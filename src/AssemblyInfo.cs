using System.Reflection;
using System.Runtime.InteropServices;

// Свойства exe (Проводник -> Свойства -> Подробно): название, версия, автор, лицензия — как у Unbound Soundtrack 1.0.4.
// 16.11.2026 (Unbound): без них антивирусы на VirusTotal давали 13/71, с ними — 3/70; код при этом не менялся. Версия — та же, что App.Version (при выпуске поднять обе).
[assembly: AssemblyTitle("Heat Soundtrack")]
[assembly: AssemblyDescription("Your own songs in Need for Speed Heat: builds one music mod for Frosty Mod Manager. Free, open source (GPL v3).")]
[assembly: AssemblyCompany("Kitivski + Claude")]
[assembly: AssemblyProduct("Heat Soundtrack")]
[assembly: AssemblyCopyright("GPL v3 - Kitivski + Claude - github.com/kotovskivan/HeatSoundtrack")]
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]
[assembly: AssemblyInformationalVersion("1.0")]
[assembly: ComVisible(false)]
