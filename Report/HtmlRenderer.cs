using System.Diagnostics;
using System.Text;
using ModChecker.Models;

namespace ModChecker.Report;

/// <summary>生成四合一 HTML 报告</summary>
public static class HtmlRenderer
{
    public static string Render(ReportView view)
    {
        var sb = new StringBuilder();
        sb.AppendLine("""
<!DOCTYPE html>
<html lang="zh">
<head>
<meta charset="utf-8">
<title>星露谷模组检测报告</title>
<style>
*{margin:0;padding:0;box-sizing:border-box}
body{font:14px/1.6 'Segoe UI','Microsoft YaHei',sans-serif;background:#0b1120;color:#cbd5e1;padding:20px}
.c{max-width:1100px;margin:0 auto}
h1{font-size:20px;margin-bottom:4px;color:#f1f5f9}
.meta{font-size:12px;color:#475569;margin-bottom:16px}

/* 统计面板 */
.grid4{display:grid;grid-template-columns:repeat(4,1fr);gap:10px;margin-bottom:16px}
.stat{background:#141e2d;border-radius:8px;padding:14px;text-align:center}
.stat .n{font-size:28px;font-weight:700}
.stat .l{font-size:10px;color:#64748b;margin-top:4px;text-transform:uppercase;letter-spacing:1px}
.stat.e .n{color:#ef4444}.stat.w .n{color:#f59e0b}.stat.p .n{color:#3b82f6}.stat.s .n{color:#a855f7}
.info-bar{background:#141e2d;border-radius:6px;padding:10px 14px;font-size:12px;color:#94a3b8;margin-bottom:16px}

/* Tab 导航 */
.tabs{display:flex;gap:2px;margin-bottom:0}
.tab-btn{padding:10px 18px;background:#1a2332;border:none;color:#64748b;cursor:pointer;font-size:13px;border-radius:8px 8px 0 0;transition:all .2s}
.tab-btn:hover{color:#cbd5e1;background:#1e293b}
.tab-btn.active{background:#141e2d;color:#f1f5f9;font-weight:600}
.tab-content{background:#141e2d;border-radius:0 8px 8px 8px;padding:16px;margin-bottom:16px;display:none}
.tab-content.active{display:block}

/* 问题条目 */
.section{background:#141e2d;border-radius:8px;padding:16px;margin-bottom:16px}
.section h3{font-size:15px;margin-bottom:12px}
.toggle-h{cursor:pointer;user-select:none}
.toggle-h:hover{color:#e2e8f0}
.toggle-arrow{font-size:12px;transition:transform .2s;display:inline-block;width:16px}
.toggle-body{display:none}
.toggle-body.open{display:block}
.empty{color:#64748b;font-size:13px;padding:8px 0}
.issue{background:#1a2332;border-radius:6px;padding:12px 14px;margin-bottom:8px}
.i-head{font-size:14px;font-weight:600;margin-bottom:4px}
.i-mod{font-size:11px;color:#475569;margin-bottom:4px}
.i-explain{font-size:13px;color:#94a3b8;margin-bottom:4px}
.i-fix{font-size:13px;color:#fbbf24;margin-bottom:6px;background:#78350f18;padding:6px 10px;border-radius:4px}
.i-code{background:#0b1120;border-radius:4px;padding:8px 12px;margin-top:6px;border:1px solid #1e293b}
.code-file{font-size:10px;color:#475569;margin-bottom:3px;font-family:monospace}
.code-snip{font-size:12px;color:#f87171;font-family:Consolas,monospace;white-space:pre-wrap;word-break:break-all}
.issue-group{margin-bottom:6px}
.group-header{cursor:pointer;padding:8px 12px;background:#1a2332;border-radius:6px;margin-bottom:4px;font-size:13px;display:flex;align-items:center;gap:6px;flex-wrap:wrap;color:#94a3b8}
.group-header:hover{background:#1e293b}
.group-count{background:#7f1d1d;color:#f87171;padding:1px 6px;border-radius:4px;font-size:11px;font-weight:600}
.group-mods{font-size:11px;color:#475569}
table{width:100%;border-collapse:collapse;font-size:13px}
th{text-align:left;padding:8px 10px;background:#1a2332;color:#94a3b8;font-weight:500;font-size:11px}
td{padding:7px 10px;border-bottom:1px solid #1e293b}
tr:hover{background:#1a2332}
.badge{display:inline-block;padding:1px 8px;border-radius:8px;font-size:10px;font-weight:600;margin-right:4px}
.badge.ok{background:#14532d;color:#4ade80}
.badge.w{background:#78350f;color:#fbbf24}
.badge.e{background:#7f1d1d;color:#f87171}
.badge.t{background:#1e3a5f;color:#60a5fa}
.q{display:inline-block;width:18px;height:18px;border-radius:50%;text-align:center;line-height:18px;font-size:10px;font-weight:700;margin-right:6px;background:#1e293b;color:#64748b}

/* 结构化输出 */
.structured-block{background:#0b1120;border:1px solid #1e293b;border-radius:6px;padding:14px;font-family:Consolas,monospace;font-size:12px;line-height:1.8;white-space:pre-wrap;color:#94a3b8}
.search-box{width:100%;padding:8px 12px;background:#0b1120;border:1px solid #1e293b;border-radius:6px;color:#cbd5e1;font-size:13px;margin-bottom:12px}
.search-box:focus{outline:none;border-color:#3b82f6}

/* 导出区块 */
.export-btn{background:#1e293b;border:1px solid #334155;color:#cbd5e1;padding:4px 12px;border-radius:4px;cursor:pointer;font-size:12px}
.export-btn:hover{background:#334155}
.export-block{background:#0b1120;border:1px solid #1e293b;border-radius:6px;padding:14px;font-family:Consolas,monospace;font-size:12px;line-height:1.7;white-space:pre-wrap;color:#94a3b8;max-height:400px;overflow-y:auto}
</style>
</head>
<body><div class="c">
""");

        // 标题
        sb.AppendLine($"<h1>星露谷物语 SMAPI 模组检测报告</h1>");
        sb.AppendLine($"<div class=\"meta\">{Encode(view.ModsPath)} &middot; {view.ScanTime:yyyy-MM-dd HH:mm} &middot; {view.ElapsedMs}ms</div>");

        // 统计面板
        sb.AppendLine($"""
<div class="grid4">
<div class="stat e"><div class="n">{view.ErrorCount}</div><div class="l">错误</div></div>
<div class="stat w"><div class="n">{view.WarningCount}</div><div class="l">警告</div></div>
<div class="stat p"><div class="n">{view.InfoCount}</div><div class="l">提示</div></div>
<div class="stat s"><div class="n">{view.ModCount}</div><div class="l">模组总数</div></div>
</div>
<div class="info-bar">检测模式: 静态预检 | SMAPI {Encode(view.SmapiVersion)} | 星露谷 {Encode(view.GameVersion)}</div>
""");

        // Tab 导航
        sb.AppendLine("""
<div class="tabs">
<button class="tab-btn active" onclick="switchTab('tab-issues')">问题明细</button>
<button class="tab-btn" onclick="switchTab('tab-export')">报错导出</button>
<button class="tab-btn" onclick="switchTab('tab-summary')">模组摘要</button>
<button class="tab-btn" onclick="switchTab('tab-table')">模组总览表</button>
</div>
""");

        // Tab1: 问题明细
        sb.AppendLine("<div id=\"tab-issues\" class=\"tab-content active\">");
        RenderIssueSection(sb, "错误", "ef4444", view.Errors);
        RenderIssueSection(sb, "警告", "d97706", view.Warnings);
        RenderIssueSection(sb, "提示信息 — 不影响游戏，可选处理", "6b7280", view.Infos);
        sb.AppendLine("</div>");

        // Tab2: 报错导出
        sb.AppendLine("<div id=\"tab-export\" class=\"tab-content\">");
        RenderExportSection(sb, "报错日志（全部输出）", view);
        RenderExportSection(sb, "简化版 + AI 提示词", view);
        RenderExportSection(sb, "开发者 MD 报告", view);
        sb.AppendLine("</div>");

        // Tab3: 模组摘要
        sb.AppendLine("<div id=\"tab-summary\" class=\"tab-content\">");
        sb.AppendLine("<input class=\"search-box\" type=\"text\" placeholder=\"搜索模组...\" oninput=\"filterSummary(this.value)\">");
        sb.AppendLine("<div id=\"summary-content\">");

        foreach (var mod in view.ModSummaries)
        {
            sb.AppendLine($"<div class=\"section summary-mod\">");
            string badgeClass = mod.Issues.Any(i => i.Severity == Severity.Error) ? "e" :
                                mod.Issues.Any(i => i.Severity == Severity.Warning) ? "w" : "t";
            sb.AppendLine($"<h3><span class=\"badge {badgeClass}\">{Encode(mod.DisplayName)}</span> <span style=\"color:#475569;font-size:12px\">{Encode(mod.UniqueID)} v{Encode(mod.Version)}</span></h3>");
            foreach (var issue in mod.Issues)
            {
                string color = issue.Severity switch
                {
                    Severity.Error => "ef4444",
                    Severity.Warning => "f59e0b",
                    _ => "3b82f6"
                };
                sb.AppendLine($"<div class=\"issue\" style=\"border-left:3px solid #{color}\">");
                sb.AppendLine($"<div class=\"i-head\"><span class=\"badge\" style=\"background:#{color}20;color:#{color}\">{Encode(issue.Category)}</span> {Encode(issue.Title)}</div>");
                if (!string.IsNullOrEmpty(issue.Explanation))
                    sb.AppendLine($"<div class=\"i-explain\"><span class=\"q\">?</span> {Encode(issue.Explanation)}</div>");
                if (!string.IsNullOrEmpty(issue.FixSuggestion))
                    sb.AppendLine($"<div class=\"i-fix\"><span class=\"q\">!</span> <strong>修复建议:</strong> {Encode(issue.FixSuggestion)}</div>");
                if (!string.IsNullOrEmpty(issue.CodeSnippet))
                    sb.AppendLine($"<div class=\"i-code\"><div class=\"code-snip\">{Encode(issue.CodeSnippet)}</div></div>");
                sb.AppendLine("</div>");
            }
            sb.AppendLine("</div>");
        }

        if (view.ModSummaries.Count == 0)
            sb.AppendLine("<div class=\"empty\" style=\"text-align:center;padding:40px\">所有模组均无问题 ✓</div>");
        sb.AppendLine("</div></div>");

        // Tab4: 模组总览表
        sb.AppendLine("<div id=\"tab-table\" class=\"tab-content\">");
        sb.AppendLine("<input class=\"search-box\" type=\"text\" placeholder=\"搜索模组...\" oninput=\"filterTable(this.value)\">");
        sb.AppendLine("""
<table id="mod-table">
<thead><tr>
<th>模组名</th><th>UniqueID</th><th>版本</th><th>目录</th><th>Manifest</th><th>Content</th><th>错误</th><th>警告</th><th>提示</th>
</tr></thead><tbody>
""");
        foreach (var row in view.ModTable)
        {
            string manifestIcon = row.HasManifest ? "✓" : "✗";
            string contentIcon = row.HasContentJson ? "✓" : "—";
            string mColor = row.HasManifest ? "#4ade80" : "#ef4444";
            sb.AppendLine($"<tr><td>{Encode(row.DisplayName)}</td><td style=\"font-size:11px;color:#64748b\">{Encode(row.UniqueID)}</td><td>{Encode(row.Version)}</td><td style=\"font-size:11px;color:#64748b\">{Encode(row.FolderName)}</td><td style=\"color:{mColor}\">{manifestIcon}</td><td style=\"color:#64748b\">{contentIcon}</td><td style=\"color:#ef4444\">{row.ErrorCount}</td><td style=\"color:#f59e0b\">{row.WarningCount}</td><td style=\"color:#3b82f6\">{row.InfoCount}</td></tr>");
        }
        sb.AppendLine("</tbody></table></div>");

        // JavaScript
        sb.AppendLine("""
<script>
function switchTab(id) {
  document.querySelectorAll('.tab-content').forEach(t => t.classList.remove('active'));
  document.querySelectorAll('.tab-btn').forEach(b => b.classList.remove('active'));
  document.getElementById(id).classList.add('active');
  event.target.classList.add('active');
}
function toggleSection(id) {
  const el = document.getElementById(id);
  const arrow = event.target.closest('.toggle-h, .group-header').querySelector('.toggle-arrow');
  if (!el) return;
  el.classList.toggle('open');
  if (arrow) arrow.style.transform = el.classList.contains('open') ? 'rotate(0)' : 'rotate(-90deg)';
}
function filterSummary(val) {
  document.querySelectorAll('.summary-mod').forEach(el => {
    el.style.display = el.textContent.toLowerCase().includes(val.toLowerCase()) ? '' : 'none';
  });
}
function filterTable(val) {
  document.querySelectorAll('#mod-table tbody tr').forEach(el => {
    el.style.display = el.textContent.toLowerCase().includes(val.toLowerCase()) ? '' : 'none';
  });
}
function copyText(id) {
  const el = document.getElementById(id);
  const text = el.textContent;
  navigator.clipboard.writeText(text).then(() => {
    const btn = event.target;
    const orig = btn.textContent;
    btn.textContent = '已复制!';
    btn.style.background = '#166534';
    setTimeout(() => { btn.textContent = orig; btn.style.background = '#1e293b'; }, 1500);
  }).catch(() => {
    const range = document.createRange();
    range.selectNode(el);
    window.getSelection().removeAllRanges();
    window.getSelection().addRange(range);
    document.execCommand('copy');
  });
}
function downloadText(id, filename) {
  const el = document.getElementById(id);
  const text = el.textContent;
  const blob = new Blob([text], { type: 'text/plain;charset=utf-8' });
  const url = URL.createObjectURL(blob);
  const a = document.createElement('a');
  a.href = url;
  a.download = filename;
  document.body.appendChild(a);
  a.click();
  document.body.removeChild(a);
  URL.revokeObjectURL(url);
}
</script>
""");

        sb.AppendLine("</div></body></html>");
        return sb.ToString();
    }

    private static void RenderIssueSection(StringBuilder sb, string title, string color, List<Issue> issues)
    {
        if (issues.Count == 0)
        {
            sb.AppendLine($"<div class=\"section\"><h3 style=\"color:#{color}\">✅ {Encode(title)} — 无</h3></div>");
            return;
        }

        // 按 Category 分组
        var groups = issues.GroupBy(i => i.Category).ToList();

        sb.AppendLine($"<div class=\"section\">");
        sb.AppendLine($"<h3 style=\"color:#{color}\">{Encode(title)} ({issues.Count})</h3>");

        foreach (var group in groups)
        {
            string groupId = $"grp-{Guid.NewGuid():N}";
            string modsList = string.Join(", ", group.Select(i => i.ModName).Distinct());

            sb.AppendLine($"<div class=\"issue-group\">");
            sb.AppendLine($"<div class=\"group-header\" onclick=\"toggleSection('{groupId}')\">");
            sb.AppendLine($"<span class=\"toggle-arrow\" style=\"transform:rotate(-90deg)\">&#9660;</span>");
            sb.AppendLine($"<span class=\"group-count\">{group.Count()}个</span>");
            sb.AppendLine($"[{Encode(group.Key)}] {Encode(group.First().Title.Split('.')[0])}");
            sb.AppendLine($"<span class=\"group-mods\">{Encode(modsList)}</span>");
            sb.AppendLine("</div>");
            sb.AppendLine($"<div id=\"{groupId}\" class=\"toggle-body\">");

            foreach (var issue in group)
            {
                sb.AppendLine($"<div class=\"issue\" style=\"border-left:3px solid #{color}\">");
                sb.AppendLine($"<div class=\"i-head\"><span class=\"badge\" style=\"background:#{color}20;color:#{color}\">{Encode(issue.Category)}</span> {Encode(issue.Title)}</div>");
                sb.AppendLine($"<div class=\"i-mod\">{Encode(issue.ModName)}</div>");
                if (!string.IsNullOrEmpty(issue.Explanation))
                    sb.AppendLine($"<div class=\"i-explain\"><span class=\"q\">?</span> {Encode(issue.Explanation)}</div>");
                if (!string.IsNullOrEmpty(issue.FixSuggestion))
                    sb.AppendLine($"<div class=\"i-fix\"><span class=\"q\">!</span> <strong>修复建议:</strong> {Encode(issue.FixSuggestion)}</div>");

                bool hasCode = !string.IsNullOrEmpty(issue.CodeSnippet);
                bool hasSource = !string.IsNullOrEmpty(issue.SourceFile);
                if (hasCode || hasSource)
                {
                    sb.AppendLine("<div class=\"i-code\">");
                    if (hasSource)
                        sb.AppendLine($"<div class=\"code-file\">{Encode(issue.SourceFile)}</div>");
                    if (hasCode)
                        sb.AppendLine($"<div class=\"code-snip\">{Encode(issue.CodeSnippet)}</div>");
                    sb.AppendLine("</div>");
                }
                sb.AppendLine("</div>");
            }

            sb.AppendLine("</div></div>");
        }

        sb.AppendLine("</div>");
    }

    /// <summary>渲染导出 Tab 中的一个报告区块</summary>
    private static void RenderExportSection(StringBuilder sb, string title, ReportView view)
    {
        string id = title switch
        {
            "报错日志（全部输出）" => "export-log",
            "简化版 + AI 提示词" => "export-ai",
            "开发者 MD 报告" => "export-md",
            _ => "export-other"
        };

        string filename = title switch
        {
            "报错日志（全部输出）" => $"mod_checker_log_{view.ScanTime:yyyyMMdd}.txt",
            "简化版 + AI 提示词" => $"mod_checker_ai_{view.ScanTime:yyyyMMdd}.txt",
            "开发者 MD 报告" => $"mod_checker_dev_{view.ScanTime:yyyyMMdd}.md",
            _ => $"mod_checker_{view.ScanTime:yyyyMMdd}.txt"
        };

        sb.AppendLine($"<div class=\"section\">");
        sb.AppendLine($"<h3 style=\"color:#94a3b8;margin-bottom:8px\">{Encode(title)}</h3>");

        // 按钮组
        sb.AppendLine("<div style=\"display:flex;gap:6px;margin-bottom:8px\">");
        sb.AppendLine($"<button onclick=\"copyText('{id}-content')\" class=\"export-btn\">复制</button>");
        sb.AppendLine($"<button onclick=\"downloadText('{id}-content','{filename}')\" class=\"export-btn\">下载文件</button>");
        sb.AppendLine("</div>");

        string content = title switch
        {
            "报错日志（全部输出）" => GenerateFullLog(view),
            "简化版 + AI 提示词" => GenerateAiPrompt(view),
            "开发者 MD 报告" => GenerateMarkdown(view),
            _ => ""
        };

        sb.AppendLine($"<pre id=\"{id}-content\" class=\"export-block\">{Encode(content)}</pre>");
        sb.AppendLine("</div>");
    }

    /// <summary>生成完整报错日志（TXT 格式）</summary>
    private static string GenerateFullLog(ReportView view)
    {
        var w = new System.Text.StringBuilder();
        w.AppendLine("================================================================");
        w.AppendLine("    星露谷物语 SMAPI 模组检测报告");
        w.AppendLine("================================================================");
        w.AppendLine($"扫描时间: {view.ScanTime:yyyy-MM-dd HH:mm:ss}");
        w.AppendLine($"模组目录: {view.ModsPath}");
        w.AppendLine($"SMAPI {view.SmapiVersion} | 星露谷 {view.GameVersion}");
        w.AppendLine($"模组总数: {view.ModCount} | 错误: {view.ErrorCount} | 警告: {view.WarningCount} | 提示: {view.InfoCount}");
        w.AppendLine();

        if (view.Errors.Count > 0)
        {
            w.AppendLine($"--- 错误 ({view.Errors.Count}) ---");
            foreach (var issue in view.Errors)
                AppendIssue(w, "错误", issue);
            w.AppendLine();
        }

        if (view.Warnings.Count > 0)
        {
            w.AppendLine($"--- 警告 ({view.Warnings.Count}) ---");
            foreach (var issue in view.Warnings)
                AppendIssue(w, "警告", issue);
            w.AppendLine();
        }

        if (view.Infos.Count > 0)
        {
            w.AppendLine($"--- 提示 ({view.Infos.Count}) ---");
            foreach (var issue in view.Infos)
                AppendIssue(w, "提示", issue);
            w.AppendLine();
        }

        return w.ToString();
    }

    /// <summary>生成简化版 + AI 提示词</summary>
    private static string GenerateAiPrompt(ReportView view)
    {
        var w = new System.Text.StringBuilder();
        w.AppendLine("【星露谷 SMAPI 模组问题诊断请求】");
        w.AppendLine("请根据以下检测结果，分析模组存在的问题并给出修复建议。");
        w.AppendLine();
        w.AppendLine("=== 环境 ===");
        w.AppendLine($"SMAPI: {view.SmapiVersion}");
        w.AppendLine($"游戏版本: {view.GameVersion}");
        w.AppendLine($"模组数量: {view.ModCount}");
        w.AppendLine($"问题统计: {view.ErrorCount} 个错误 / {view.WarningCount} 个警告 / {view.InfoCount} 个提示");
        w.AppendLine();

        if (view.Errors.Count > 0)
        {
            w.AppendLine("=== 错误列表 ===");
            int i = 1;
            foreach (var issue in view.Errors)
            {
                w.AppendLine($"  {i}. [{issue.ModName}] {issue.Title}");
                i++;
            }
            w.AppendLine();
        }

        if (view.Warnings.Count > 0)
        {
            w.AppendLine("=== 警告列表 ===");
            int i = 1;
            foreach (var issue in view.Warnings)
            {
                w.AppendLine($"  {i}. [{issue.ModName}] {issue.Title}");
                i++;
            }
            w.AppendLine();
        }

        w.AppendLine("请给出以下内容：");
        w.AppendLine("1. 哪些问题需要优先处理");
        w.AppendLine("2. 每个问题的修复步骤");
        w.AppendLine("3. 可能影响游戏运行的严重问题");
        w.AppendLine();

        return w.ToString();
    }

    /// <summary>生成开发者 MD 报告</summary>
    private static string GenerateMarkdown(ReportView view)
    {
        var w = new System.Text.StringBuilder();
        w.AppendLine("# 星露谷 SMAPI 模组检测报告");
        w.AppendLine();
        w.AppendLine("## 环境信息");
        w.AppendLine();
        w.AppendLine($"| 项目 | 值 |");
        w.AppendLine($"|------|-----|");
        w.AppendLine($"| SMAPI | {view.SmapiVersion} |");
        w.AppendLine($"| 游戏版本 | {view.GameVersion} |");
        w.AppendLine($"| 模组总数 | {view.ModCount} |");
        w.AppendLine($"| 检测时间 | {view.ScanTime:yyyy-MM-dd HH:mm:ss} |");
        w.AppendLine($"| 耗时 | {view.ElapsedMs}ms |");
        w.AppendLine();
        w.AppendLine($"## 问题统计");
        w.AppendLine();
        w.AppendLine($"- 🔴 错误: {view.ErrorCount}");
        w.AppendLine($"- 🟡 警告: {view.WarningCount}");
        w.AppendLine($"- 🔵 提示: {view.InfoCount}");
        w.AppendLine();

        if (view.Errors.Count > 0)
        {
            w.AppendLine("## 错误详情");
            w.AppendLine();
            foreach (var issue in view.Errors)
            {
                w.AppendLine($"### [{issue.ModName}] {issue.Title}");
                w.AppendLine();
                if (!string.IsNullOrEmpty(issue.Explanation))
                    w.AppendLine($"- **说明**: {issue.Explanation}");
                if (!string.IsNullOrEmpty(issue.FixSuggestion))
                    w.AppendLine($"- **修复建议**: {issue.FixSuggestion}");
                if (!string.IsNullOrEmpty(issue.SourceFile))
                    w.AppendLine($"- **位置**: `{issue.SourceFile}`");
                if (!string.IsNullOrEmpty(issue.CodeSnippet))
                    w.AppendLine($"- **代码**: `{issue.CodeSnippet}`");
                w.AppendLine();
            }
        }

        if (view.Warnings.Count > 0)
        {
            w.AppendLine("## 警告详情");
            w.AppendLine();
            foreach (var issue in view.Warnings)
            {
                w.AppendLine($"### [{issue.ModName}] {issue.Title}");
                w.AppendLine();
                if (!string.IsNullOrEmpty(issue.Explanation))
                    w.AppendLine($"- **说明**: {issue.Explanation}");
                if (!string.IsNullOrEmpty(issue.FixSuggestion))
                    w.AppendLine($"- **修复建议**: {issue.FixSuggestion}");
                w.AppendLine();
            }
        }

        w.AppendLine("## 模组清单");
        w.AppendLine();
        w.AppendLine("| 模组 | UniqueID | 版本 | 错误 | 警告 |");
        w.AppendLine("|------|----------|------|------|------|");
        foreach (var row in view.ModTable)
        {
            w.AppendLine($"| {row.DisplayName} | {row.UniqueID} | {row.Version} | {row.ErrorCount} | {row.WarningCount} |");
        }
        w.AppendLine();
        w.AppendLine("---");
        w.AppendLine($"*由 SMAPI Mod Checker v3 自动生成*");

        return w.ToString();
    }

    private static void AppendIssue(System.Text.StringBuilder w, string label, Issue issue)
    {
        w.AppendLine($"  [{label}] [{issue.Category}] {issue.Title}");
        w.AppendLine($"    模组: {issue.ModName}");
        if (!string.IsNullOrEmpty(issue.Explanation))
            w.AppendLine($"    说明: {issue.Explanation}");
        if (!string.IsNullOrEmpty(issue.FixSuggestion))
            w.AppendLine($"    建议: {issue.FixSuggestion}");
        if (!string.IsNullOrEmpty(issue.CodeSnippet))
            w.AppendLine($"    >>> {issue.CodeSnippet}");
        w.AppendLine();
    }

    private static string Encode(string? text) =>
        System.Net.WebUtility.HtmlEncode(text ?? "");
}
