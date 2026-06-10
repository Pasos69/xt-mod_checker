# 更新日志

## v3.0 — 2026-06-10

### 重大重构

从 v2（纯日志解析 + 4 文件输出）全面重构为 v3（真实 SMAPI 引擎 + 单 HTML 报告）。

---

### 🆕 新增功能

#### 检测引擎
- **SMAPI.Toolkit 真实解析** — 反射加载 JsonHelper.Deserialize\<T\>()，manifest 解析与 SMAPI 完全一致
- **依赖图检测** — 遍历 Dependencies，标记缺失依赖和 ContentPackFor 链断裂
- **UniqueID 冲突检测** — 全库哈希比对，重复 ID 直接报错
- **缺失 EntryDll 检测** — 验证 manifest 中指定的 DLL 文件是否存在
- **ContentPackFor 有效性** — 检查引用的框架模组是否实际安装
- **XNB 替换检测** — 扫描模组目录中的 .xnb 文件（基础版）
- **SMAPI/游戏版本读取** — 直接读 DLL/EXE 文件的 FileVersion，不再靠日志行号

#### 报告输出
- **单 HTML 四 Tab** — 问题明细 / 报错导出 / 模组摘要 / 模组总览表，替代 v2 的 4 文件
- **三格式一键导出** — 每个格式支持复制 + 下载文件：
  - 报错日志（完整 TXT）
  - 简化版 + AI 提示词（TXT）
  - 开发者 MD 报告（Markdown）
- **默认折叠** — 问题组默认收起，不会一打开就被满屏红色吓到
- **搜索筛选** — 每个 Tab 支持实时搜索过滤
- **可下载文件名带日期** — 如 `mod_checker_log_20260610.txt`

#### 用户体验
- **双击 / 拖拽都支持** — exe 放 Mods 双击，或拖拽任意 Mods 目录到 exe
- **自动浏览器打开** — 扫描完自动弹出 HTML 报告
- **控制台 UTF-8** — 中文不乱码
- **自定义图标** — 深底蓝框绿色勾 ✓，内嵌 exe 不失效

---

### ⚡ 优化

| 项目 | v2 | v3 |
|------|----|----|
| 输出数量 | 1 HTML + 3 TXT | 1 HTML（四 Tab 内切换） |
| 检测方式 | SMAPI-latest.txt 行号匹配 | SMAPI.Toolkit 反射加载 + 文件系统扫描 |
| 检测维度 | ~6 类 | ~22 类 |
| 体积 | ~60MB | ~65MB（含真实 SMAPI 引擎） |
| 路径发现 | 固定目录 | 自动 Mods + 拖拽 |
| 报告可读性 | 无折叠/搜索 | 折叠 + 搜索 + 导出 |

---

### 🔧 技术变更

- **语言**：C# .NET 8.0
- **发布**：自包含单文件
- **依赖**：Newtonsoft.Json（基础解析）+ SMAPI.Toolkit 反射加载（优先引擎）
- **无裁剪发布** — SMAPI.Toolkit 需要 netstandard.dll，不可裁剪
- **项目重命名**：V3.exe（含 app.ico 图标）

---

### 🗑️ 移除

- SMAPI-latest.txt 日志解析（不稳定，全文移除）
- 四文件分散输出
- update_available / game_patcher 等日志正则匹配
- 所有离线日志分析逻辑

---

### 📁 项目文件

```
D:\2025\MOD\AI工作区\mod_checker_v3\
├── Program.cs                 入口
├── CLAUDE.md                  编码规范
├── CHANGELOG.md               本文件
├── SMAPI_Mod_Checker_v3_方案.md  方案文档
├── app.ico                    应用图标
├── Scanner/
│   ├── PathResolver.cs        路径发现
│   ├── ModEnumerator.cs       模组枚举
│   ├── ManifestAnalyzer.cs    manifest 分析
│   ├── ContentAnalyzer.cs     content.json 分析
│   ├── DependencyGraph.cs     依赖图
│   ├── XnbAnalyzer.cs         XNB 检测（v3.1 待增强）
│   └── SmapiToolkitLoader.cs  SMAPI 引擎加载
├── Report/
│   ├── ReportModel.cs         报告聚合
│   └── HtmlRenderer.cs        HTML 生成
└── Models/
    ├── Manifest.cs
    ├── ContentEntry.cs
    └── ScanResult.cs
```
