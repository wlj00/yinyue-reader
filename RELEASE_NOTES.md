# 隐阅 YinYue · 发布说明 Release Notes

## 版本 v1.1（2026-11-27）

### 新增：全部键盘快捷键可自定义

这条来自真实用户反馈：*“用了发现顶部栏隐藏不了。本来有软件占了 F1，把那软件关掉之后还是隐藏不了顶栏。”*

原因很实在——Windows 把 `F1` 当成系统帮助键，优先级比你程序里的热键更高，所以程序注册不上它，按了没反应，而旧版并没有把这事告诉用户。

这一版的做法是：**别跟系统抢，直接给用户换键的权利。**

#### 改动内容

- 顶栏新增 **「键位 / Keys」** 按钮（位于「章节」和「打赏」之间）
- 点开后是「快捷键设置」窗口，**10 项键盘快捷键全都能改**，分两页：
  - **全局热键**（任何时候都生效）：隐藏 / 唤出窗口、显示 / 隐藏顶栏
  - **窗口内快捷键**（窗口有焦点时生效）：自动滚动开关、上翻页、下翻页、整页上翻、整页下翻、字号增大、字号减小、章节列表
  - 点一下输入框，**直接按下你想用的键**就录进去了；支持单键、`Ctrl / Alt / Shift / Win` 组合键，支持 `F1–F24`、字母、数字、`~ - = [ ] ; ' , . / \` 以及方向键、Home/End/PgUp/PgDn/空格/回车/Esc/Tab/退格等
  - **保存并生效**：立刻重新注册热键，无需重启程序
  - **恢复默认**：一键把 10 项全部还原（`~` 隐藏窗口 / `F1` 顶栏 / 空格 自动滚动 / ↑↓ 翻页 / PgUp PgDn 整页 / Ctrl± 字号 / Ctrl+T 章节）
  - 窗口内直接提醒「F1 常被 Windows 系统帮助占用，建议改成 Ctrl+Shift+H 这类带修饰键的组合」
- **保存时会做三重校验**：格式是否合法、任意两项是否撞键（会指出具体是哪两项）、全局热键能否真正注册成功
- **失败自动回滚**：如果全局热键被别的软件占用而注册失败，会弹窗说明，并自动退回你原来的键位重新注册，绝不会让你「改完就唤不出窗口」
- 键位自动写入程序同目录 `reader_config.txt`（`hideKey` / `barKey` / `scrollKey` / `upKey` / `downKey` / `pgUpKey` / `pgDnKey` / `zoomInKey` / `zoomOutKey` / `chapterKey`），下次启动沿用；旧配置文件不受影响，缺失项自动取默认值
- 帮助窗改成可滚动面板，按当前实际键位实时渲染 10 行快捷键说明
- 顶栏改为更紧凑的排布以容纳 9 个按钮，最小窗口宽度从 300 调到 380，避免按钮文字被挤压
- **修复打赏码扫不出来**：旧版把带大片背景的宣传图整张缩到 160×220，二维码实际只剩约 55px，手机识别不到。现在图片裁成纯二维码（保留静区），窗口按 **340px** 显示微信 / 支付宝两枚码，并分别用绿/蓝字体标注「微信」「支付宝」；打赏窗整体放大到 800×508，附「扫一扫」提示。

#### 🎮 快捷键（★★ 全部可自定义）
★★`~` 隐藏/显示窗口｜★★`F1` 隐藏顶栏｜★★`空格` 自动滚动｜★★`↑↓` 翻页｜★★`PgUp/PgDn` 整页｜★★`Ctrl+/-` 字号｜★★`Ctrl+T` 章节｜`Ctrl+滚轮` 字号｜滚轮翻页

#### 📦 运行环境
Windows 7 / 8 / 10 / 11（.NET Framework 4.0，免安装单文件）

#### 🔓 许可证
个人免费使用，禁止商用。

---

### English — v1.1 adds fully customizable keyboard shortcuts

A user reported that the toolbar wouldn't hide: closing the app that occupied `F1` didn't help either. Root cause: Windows reserves `F1` for help and takes it before third-party hotkeys — and the old build never reported the failed registration.

This version stops fighting the system and hands the key choice to you:

- New **Keys** button in the toolbar (between Chapters and Tip)
- Opens a **Hotkey Settings** dialog covering **all 10 keyboard shortcuts**, across two tabs: global hotkeys (hide/show window, show/hide toolbar) and in-app shortcuts (auto scroll, page up/down, full page, font size, chapter list)
- Click a box, press the combination you want (`Ctrl+Shift+H`, `Alt+Z`, `F8`, `Shift+Up`, …)
- **Save & Apply** re-registers immediately, no restart; **Restore Defaults** resets all 10
- Validates format, names the two actions when a combo is duplicated, and if a global hotkey can't be registered the old one is restored automatically
- Bindings are stored in `reader_config.txt` next to the exe (`hideKey` / `barKey` / `scrollKey` / `upKey` / `downKey` / `pgUpKey` / `pgDnKey` / `zoomInKey` / `zoomOutKey` / `chapterKey`); existing config files keep working
- Help window now scrolls and renders the shortcut list from your live bindings
- **Fixed unscannable donation QR**: the old build shrank a full promo image into 160×220, leaving the code at ~55px. The images are now cropped to the bare code (quiet zone kept) and shown at **340px** each, labelled WeChat (green) / Alipay (blue), in an 800×508 dialog.

---

## 历史版本 · v1.0（首次公开发布 / Initial Public Release）

**版本 v1.0**（首次公开发布 / Initial Public Release）

---

## 中文版

### 隐阅 · 透明悬浮小说阅读器 v1.0

一款让文字直接浮在桌面上的极简阅读器。工作摸鱼两不误，任何窗口之上都能阅读。

#### ✨ 主要特性
- **完全透明 + 始终置顶**：文字浮在桌面上，叠加在 PS、浏览器等任何窗口之上
- **纯黑/纯白文字，无描边**：更隐蔽，浅色背景用黑字、深色背景用白字，一键切换
- **阅读进度 %**：右下角实时显示，随滚动位置准确变化
- **章节跳转**：自动识别「第X章」，精确跳到对应章节
- **编码自适应**：UTF-8 / UTF-16 / GBK / GB2312 自动识别，中文不乱码
- **中英文界面**、黑白文字切换
- **进度 & 窗口记忆**：下次打开回到上次位置
- **流畅不卡顿**：打开大文件秒开，拖动窗口不闪不重影，F1 切换顶栏瞬切
- **自动滚动**、键盘/滚轮翻页、字号调节
- **意见反馈**：顶栏「反馈」按钮弹出邮箱（1051783236@qq.com），一键复制
- **内嵌打赏码**：微信 + 支付宝，单文件分发

#### 🎮 快捷键
`~` 隐藏/显示｜`F1` 隐藏顶栏｜`空格` 自动滚动｜`↑↓` 翻页｜`Ctrl+/-` 字号｜`Ctrl+T` 章节｜`Ctrl+滚轮` 字号｜滚轮翻页

#### 📦 运行环境
Windows 7 / 8 / 10 / 11（自带 .NET Framework，免安装）

#### 🔓 许可证
**个人免费使用，禁止商用**。详见使用说明。

#### 🧪 已知事项
- 本工具为本地 TXT 阅读器，不含任何小说内容，请勿用于传播未授权版权内容
- 透明窗口基于系统抠色实现，个别极特殊桌面配色下可能出现轻微边缘现象

---

## English

### YinYue — Stealth Floating Novel Reader v1.0

A minimalist reader that floats text directly on your desktop. Read while you work — it sits on top of any window.

#### ✨ Highlights
- **Fully transparent + always-on-top**: text floats over PS, browsers, anything
- **Pure white/black text, no outline**: stealthier, toggle B/W one-click
- **Reading progress %**: shown at bottom-right, accurate as you scroll
- **Chapter jump**: auto-detects chapters, jumps precisely
- **Auto encoding**: UTF-8 / UTF-16 / GBK / GB2312, Chinese never garbled
- **Chinese / English UI**, black/white text toggle
- **Smooth & fast**: opens large files instantly, no flicker or ghosting when dragging, F1 toolbar toggle is instant
- **Remembers position & progress**: resume where you left off
- **Auto scroll**, keyboard/wheel paging, font size adjust
- **Feedback**: toolbar button opens email (1051783236@qq.com), one-click copy
- **Built-in donation QR**: WeChat + Alipay, single-file distribution

#### 🎮 Shortcuts
`~` hide/show｜`F1` hide toolbar｜`Space` auto scroll｜`↑↓` page｜`Ctrl+/-` font size｜`Ctrl+T` chapters｜`Ctrl+wheel` font size｜wheel page

#### 📦 Requirements
Windows 7 / 8 / 10 / 11 (.NET Framework included, no install)

#### 🔓 License
**Free for personal use, commercial use prohibited.**

#### 🧪 Known Notes
- Local TXT reader only; contains no novels. Do not use to spread unauthorized copyrighted content.
- Transparency uses OS color-keying; slight edge artifacts possible on very unusual desktop colors.
