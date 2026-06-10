# CLAUDE.md

## Build & Publish

```bash
# 开发构建
dotnet build

# 发布单文件 exe（产出在 publish/ 目录）
dotnet publish -c Release -o publish

# 直接运行（调试用）
dotnet run
```

## 项目架构

```
mod_checker                # 单文件自包含 .NET 8 控制台应用
├── Program.cs             # 入口：路径发现 + 协调各扫描模块 + 输出 HTML
├── Scanner/
│   ├── PathResolver.cs    # 路径策略：必须在 Mods/ 内运行，或拖拽目录到 exe
│   ├── ModEnumerator.cs   # 枚举模组目录，识别 manifest.json
│   ├── ManifestAnalyzer.cs# 解析 manifest.json（用 Newtonsoft.Json，不用 SMAPI 程序集）
│   ├── ContentAnalyzer.cs # 解析 content.json，检测缺失文件 + Patch Target 重叠
│   ├── XnbAnalyzer.cs     # 解析 Content/Data/*.xnb，做 XNB 覆盖冲突检测
│   └── DependencyGraph.cs # 构建依赖图，检测断裂链和循环依赖
├── Report/
│   ├── ReportModel.cs     # 统一数据模型（Issue, Severity, Category, ModIssue）
│   └── HtmlRenderer.cs    # 生成四合一 HTML（CSS/JS 全内联，零外部依赖）
└── Models/
    ├── Manifest.cs         # manifest.json POCO
    ├── ContentEntry.cs     # content.json Patch POCO
    └── ScanResult.cs       # 扫描结果聚合模型
```

## 核心设计决策

1. **零依赖 SMAPI** — 不引用 SMAPI.Toolkit 程序集，直接 Newtonsoft.Json 解析 JSON 格式的 manifest
2. **不解析日志** — 不做 SMAPI-latest.txt 文本匹配，所有检测基于文件系统扫描 + 直接读取
3. **输出单 HTML** — 四个 Tab 页替代原来四个文件（问题明细/结构化/模组摘要/总览表）
4. **固定 Mods 目录** — exe 必须放在 Mods/ 目录下运行，或通过拖拽指定目录
5. **自包含发布** — 所有依赖打包进单 exe，用户无需安装 .NET 运行时

## 检测模块职责

### ManifestAnalyzer
- UniqueID 唯一性、必填字段、EntryDll 存在性
- UpdateKeys 格式验证

### ContentAnalyzer  
- content.json JSON 语法校验
- Patch Target 引用文件存在性
- Patch Target 重叠检测（两个模组改同一路径）

### DependencyGraph
- ContentPackFor 链完整性
- 版本范围兼容性
- 循环依赖

### XnbAnalyzer
- 用嵌入式 pyxnb 或 MonoGame 工具解析 XNB
- 比对两个模组是否覆盖同一 XNB 文件
- Patch 目标是否在游戏原始文件清单中

## 执行流程

```
Program.Main()
  ├── PathResolver.Resolve() → 确定游戏根目录 + Mods 路径
  ├── ModEnumerator.Enumerate() → 扫描所有模组
  ├── 并行执行:
  │   ├── ManifestAnalyzer.Analyze()
  │   ├── ContentAnalyzer.Analyze()
  │   ├── DependencyGraph.Build()
  │   └── XnbAnalyzer.Analyze()
  ├── ReportModel.Aggregate() → 汇总结果
  ├── HtmlRenderer.Render() → 生成 HTML
  └── Process.Start(browser, html) → 自动打开
```

## 命名规范

- 类/方法: PascalCase
- 参数/局部变量: camelCase
- 文件/目录: PascalCase
- 异步方法: Async 后缀
- 接口: I 前缀
