# Отчёт: калькулятор Bakhmatov (C# WPF, .NET 8)

## 1. Назначение

Настольное приложение-калькулятор с вычислением арифметических выражений, историей операций, светлой/тёмной темой и локализацией интерфейса (RU/EN). Логика вынесена в отдельную библиотеку; интерфейс построен по MVVM.

## 2. Технологии

| Компонент | Технология |
|-----------|------------|
| Платформа | .NET 8 |
| UI | WPF (`net8.0-windows`) |
| Библиотека | Class Library (`net8.0`) |
| Тесты | xUnit |
| Сериализация | System.Text.Json |

## 3. Структура решения

```
BakhmatovCalculator/
├── BakhmatovCalculator.sln
├── Report.md
├── BakhmatovCalculatorLib/          # Библиотека классов
│   ├── Calculators/
│   │   ├── CalculatorEngine.cs
│   │   └── ReversePolishNotation.cs
│   ├── Models/
│   │   ├── CalculationHistoryItem.cs
│   │   └── ThemeSettings.cs
│   └── Services/
│       ├── HistoryService.cs
│       └── SettingsService.cs
├── BakhmatovCalculatorApp/          # WPF-приложение
│   ├── ViewModels/
│   │   └── MainViewModel.cs
│   ├── Infrastructure/
│   │   └── RelayCommand.cs
│   ├── MainWindow.xaml / .cs
│   ├── App.xaml / App.xaml.cs
│   └── App.config
└── BakhmatovCalculatorTests/
    └── CalculatorEngineTests.cs
```

## 4. Библиотека `BakhmatovCalculatorLib`

### 4.1 Парсинг и вычисление

- Реализован алгоритм **shunting-yard** (перевод в обратную польскую запись) в `ReversePolishNotation`.
- Поддерживаются операторы: `+`, `-`, `*`, `/`, `^` (целая степень).
- Скобки `(` `)`.
- **Унарный минус** (в начале выражения и после оператора/скобки).
- Результат — `decimal`; при делении на ноль — `DivideByZeroException`; при переполнении — `OverflowException`.

### 4.2 Модели

- **CalculationHistoryItem** — `Expression`, `Result`, `Timestamp`.
- **ThemeSettings** — `IsDarkTheme`, `AccentColor`.

### 4.3 Сервисы

- **HistoryService** — загрузка/сохранение истории в JSON (по умолчанию `%LocalAppData%\BakhmatovCalculator\history.json`).
- **SettingsService** — загрузка/сохранение настроек темы в JSON (`settings.json` в том же каталоге).

### 4.4 CalculatorEngine

- `Calculate(string)` — вычисление и запись в историю.
- `Evaluate(string)` — вычисление без добавления в историю (для предпросмотра из истории).
- Методы работы с историей и темой, `SaveAll()` для сохранения на диск.

## 5. Приложение `BakhmatovCalculatorApp`

### 5.1 Архитектура

- **MVVM**: `MainViewModel`, команды через `RelayCommand`, привязки в XAML.
- Точка входа: `MainWindow`, `DataContext` задаётся в коде окна.

### 5.2 Интерфейс

- Плоский (flat) дизайн с закруглёнными элементами.
- Цифровая клавиатура, операторы, `C`, `CE`, `=`, `<-`.
- Панель истории справа с прокруткой; элементы кликабельны.
- Переключение **тёмной/светлой** темы; accent-цвет из настроек.
- Анимации кнопок (hover / press) с easing.
- **Локализация RU/EN**: заголовок окна — «Калькулятор Бахматова» / «Calculator by Bakhmatov»; подписи интерфейса переключаются кнопкой языка.

### 5.3 Ввод

- Мышь — кнопки калькулятора.
- Клавиатура — цифры, операторы, точка/запятая, скобки; Enter — вычисление; Backspace — удаление символа; Escape — очистка.

### 5.4 Сохранение данных

- При закрытии окна вызывается сохранение истории и настроек через движок.

## 6. Тесты `BakhmatovCalculatorTests`

Покрытие (xUnit):

- Базовые операции `+`, `-`, `*`, `/`, `^`.
- Приоритет операций и скобки.
- Правоассоциативность степени.
- Унарный минус.
- Деление на ноль и переполнение.

Запуск:

```bash
dotnet test BakhmatovCalculator.sln -c Release
```

## 7. Сборка и запуск

```bash
dotnet build BakhmatovCalculator.sln -c Release
dotnet run --project BakhmatovCalculatorApp/BakhmatovCalculatorApp.csproj
```

В **Visual Studio** в качестве запускаемого проекта нужно выбрать **BakhmatovCalculatorApp** (не библиотеку).

## 8. Автор и версия

Проект: Calculator by Bakhmatov. 

Автор: Бахматов Матвей
 
Документ `Report.md` описывает состояние решения на момент последнего обновления репозитория.
