# Использование AI

## Какие AI-инструменты использовались

- **Codex CLI** — модель `openai/gpt-5.6-luna` через **ProxyAPI** (российский OpenAI-совместимый шлюз). Основной инструмент разработки.
- **Qwen Code** (Qwen3-Coder через OpenRouter) — использовался на начальных этапах. От него остались каркас папок, asmdef и начальные Domain-классы. Заменён на Codex из-за проблем с OpenRouter (лимиты, резервирование кредитов, блокировки).
- **DeepSeek** — использовался для мелких задач, анализа некоторых проблем и поиска информации, также помощь в создании промптов

## Какие задачи передавались AI

- Генерация Domain-слоя: `MachineModel`, `CostCalculator`, `EconomyService`, `BoostService`, `OfflineProgressCalculator`.
- Генерация сервисов: Storage, Analytics, IAP.
- Генерация Presentation-слоя и Composition Root.
- Интеграция Unity IAP v5. Codex проверил актуальные сигнатуры пакета 5.4.4 в локальном `Library/PackageCache`.
- Интеграция Firebase Remote Config и Firebase Analytics с fallback и graceful degradation.
- Интеграция AppMetrica через OpenUPM.
- Написание всех EditMode и PlayMode тестов.
- Ревизия и исправление кода, сгенерированного другим AI.
- Документация (README, AI_USAGE).

## Ключевые промпты

- «Прочитай AGENTS.md, TEST_TASK.md и DEVELOPMENT_PLAN.md. Составь пошаговый план разработки от пустого проекта до сдачи».
- «Реализуй Domain-слой Mini Factory (чистый C#, без UnityEngine): MachineModel, CostCalculator, EconomyService, BoostService, OfflineProgressCalculator».
- «Проверь и исправь offline-прогресс и работу Boost при сворачивании приложения».
- «Интегрируй Unity IAP v5 через event-driven StoreController и Fake Store. Если не уверен в сигнатурах — проверь в локальном пакете и скажи мне».
- «Создай RemoteConfigProvider с fallback на локальный GameConfigSO и условной компиляцией FIREBASE_REMOTE_CONFIG_ENABLED».
- «Оберни AppMetrica и Firebase в try/catch, чтобы исключения SDK не ломали GameBootstrap».

## Что реализовано с помощью AI

Почти вся кодовая база:

- Domain-слой и экономические расчёты.
- Config-слой и RemoteConfigProvider.
- Storage, Analytics (три провайдера), IAP-сервисы.
- Presentation/UI-компоненты.
- Persistence и GameBootstrap.
- 18 EditMode-тестов и 5 PlayMode-тестов.
- README и AI_USAGE.

## Что сделано самостоятельно

- Настройка окружения: Node.js, Codex CLI, ProxyAPI, VPN, Git, Git LFS.
- Ручная сборка UI-сцены в Unity Editor (Canvas, панели, кнопки, привязка компонентов).
- Создание и настройка `GameConfig.asset` (3 машины + параметры).
- Подключение TextMeshPro и Unity IAP v5 через Package Manager.
- Установка Firebase SDK через `.unitypackage` (App, Remote Config, Analytics).
- Настройка Firebase Console: создание проекта, добавление Android-приложения, скачивание `google-services.json`, создание параметров Remote Config.
- Установка AppMetrica через OpenUPM.
- Настройка EDM4U + JAVA_HOME для Android-резолва.
- Сборка APK и тестирование на устройстве realme RMX5078.
- Профилирование Android build через Unity Profiler на устройстве.
- Финальный review кода.

## Отклонённые или изменённые предложения AI

- **IAP v4 вместо v5**: Codex изначально предложил старый API (`ConfigurationBuilder`, `IStoreListener`). Отклонено — переписано под v5 (`StoreController`, `OnPurchasePending`, `ConfirmPurchase`).
- **DI-контейнер Zenject**: предложение отклонено из-за масштаба проекта и требования ТЗ избегать overengineering.
- **Метод-алиас `TickProduction` для тестов**: заменено на использование `TryTickProduction` как единого публичного метода.
- **`GetProductById` в IAP v5**: Codex ошибся, сам обнаружил и заменил на `GetProductByCatalogListingId`.
- **Немедленный `OnPurchaseSuccess` в `OnPurchasePending`**: неправильный порядок — успех должен объявляться только после `OnPurchaseConfirmed`. Исправлено.

## Ошибки, обнаруженные в AI-коде

- **Qwen** ошибочно рассчитывал производство машины от стоимости улучшения (`BaseUpgradeCost`). Исправлено на `BaseProduction`.
- **Qwen** использовал формулу upgrade со смещением `level - 1`. Исправлено на формулу с текущим `level`.
- **Codex** использовал `IStoreController` (интерфейс) вместо `StoreController` (класс) — сам обнаружил при проверке пакета.
- **Codex** первоначально не подписался на `OnPurchaseConfirmed` и `OnPurchaseDeferred`. Подписки добавлены после проверки исходников пакета.
- **Codex** не учёл lifecycle Boost при возврате из паузы — прошедшее время не списывалось с длительности буста. Добавлен вызов `BoostService.Tick(elapsed)` в `OnApplicationPause`.
- **Codex** использовал `?.` для проверки View, что не работает с уничтоженными Unity-объектами — заменено на явные `if (view != null)`.
- **Codex** не реализовал graceful degradation для SDK — при невалидном AppMetrica API key на реальном устройстве падала вся инициализация `GameBootstrap`. Добавлены try/catch и degraded mode.

## Итоговый вывод по использованию AI

AI закрыл 90% рутинной работы: генерация шаблонов, ревизия чужого кода, поиск актуальных сигнатур пакетов. Самостоятельная работа потребовалась там, где AI был уверен в устаревших данных (IAP v4 вместо v5) или не учитывал Unity-специфику (`?.` vs `!= null`, `Object.Destroy` и MissingReferenceException, Editor vs Runtime поведение SDK).

Главная ценность — возможность проверить фактический API в локальном `PackageCache` и переписать сгенерированный код под реальность. Это итеративный процесс: AI пишет → я проверяю в Unity → AI фиксит по конкретной ошибке.
