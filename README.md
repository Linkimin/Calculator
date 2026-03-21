# 🧮 Calculator by Bakhmatov

Настольный калькулятор на C# / WPF (.NET 8) с вычислением арифметических выражений, историей операций и поддержкой тёмной/светлой темы.

![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=flat-square&logo=dotnet)
![WPF](https://img.shields.io/badge/UI-WPF-0078D4?style=flat-square&logo=windows)
![xUnit](https://img.shields.io/badge/Tests-xUnit-green?style=flat-square)
![License](https://img.shields.io/badge/License-MIT-blue?style=flat-square)

## ✨ Возможности

- Вычисление арифметических выражений: `+` `-` `*` `/` `^` и скобки
- Унарный минус (`-5+2`, `3*-2`)
- Десятичные числа — точка или запятая как разделитель
- Предпросмотр результата в реальном времени
- История вычислений с возможностью восстановить выражение
- Тёмная и светлая тема с сохранением настройки
- Локализация интерфейса RU / EN
- Ввод с клавиатуры: цифры, операторы, Enter, Backspace, Escape

## 🏗️ Архитектура
```
BakhmatovCalculator/
├── BakhmatovCalculatorLib/       # Библиотека классов (бизнес-логика)
│   ├── Calculators/              # Shunting-yard алгоритм, CalculatorEngine
│   ├── Models/                   # CalculationHistoryItem, ThemeSettings
│   ├── Services/                 # HistoryService, SettingsService (JSON)
│   └── Infrastructure/           # AppPaths
├── BakhmatovCalculatorApp/       # WPF-приложение
│   ├── ViewModels/               # MainViewModel (MVVM)
│   └── Infrastructure/           # RelayCommand, StringToBrushConverter
└── BakhmatovCalculatorTests/     # xUnit тесты (25+)
```

**Ключевые решения:**
- Алгоритм **Shunting-yard** для разбора и вычисления выражений
- Тип `decimal` для точных вычислений без floating-point ошибок
- **MVVM** — ViewModel не зависит от WPF (`Brush` заменён на HEX-строки + конвертер)
- Иммутабельная модель `CalculationHistoryItem` с `[JsonConstructor]`
- История и настройки персистируются в `%LocalAppData%\BakhmatovCalculator\`

## 🚀 Запуск

### Готовый билд
Скачай последний релиз со страницы [Releases](../../releases) и запусти `BakhmatovCalculatorApp.exe`.

### Из исходников
```bash
git clone https://github.com/Linkimin/Calculator.git
cd Calculator
dotnet build BakhmatovCalculator.sln -c Release
dotnet run --project BakhmatovCalculatorApp/BakhmatovCalculatorApp.csproj
```

Либо открой `BakhmatovCalculator.sln` в Visual Studio 2022 и запусти `BakhmatovCalculatorApp`.

## 🧪 Тесты
```bash
dotnet test BakhmatovCalculator.sln -c Release
```

Покрытие включает базовые операции, приоритет, унарный минус, граничные случаи степени, негативные кейсы и персистентность.

## 🛠️ Требования

- Windows 10 / 11
- .NET 8 SDK (для сборки из исходников)

## 👤 Автор

**Бахматов Матвей** — РЭУ им. Плеханова, Пермский филиал
