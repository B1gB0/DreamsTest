# Mini Factory — тестовое задание Unity Developer

## О проекте

Небольшой мобильный Unity-проект: производственная фабрика с экономикой, временными Boost, offline-прогрессом, сохранением, Unity IAP v5, и аналитикой через три провайдера.

**Технические требования:**
- Unity 6000.3.22f1
- Платформа: Android (IL2CPP, ARM64, Minimum API 23)
- Unity IAP v5 (Fake Store)
- Analytics: Console + AppMetrica + Firebase Analytics
- Remote Config: Firebase Remote Config с fallback на локальный GameConfigSO

## Как открыть проект

1. Открыть проект в Unity Hub (версия 6000.3.22f1).
2. Дождаться импорта пакетов и LFS-файлов Firebase (5–10 минут в первый раз).
3. Открыть сцену `Assets/_Project/Scenes/Main.unity`.
4. Нажать `Play`.

## Как собрать Android build

1. `File → Build Profiles → Android → Switch Platform`.
2. Проверить настройки (`Edit → Project Settings → Player → Android`):
   - Scripting Backend = IL2CPP
   - Target Architectures = ARM64 (обязательно)
   - Minimum API Level = 23 (Android 6.0)
   - Managed Stripping Level = Minimal (или Low)
   - Package Name = com.b1gb0.dreamstest (или любой другой)
3. `File → Build Profiles → Build` → сохранить APK в `Builds/`.
4. Для профилирования: включить `Development Build` + `Autoconnect Profiler`, затем `Build and Run`.

**Известные проблемы при сборке:**
- Firebase SDK может запросить выбор `google-services.json` при первом открытии — нажать `Apply`.
- Если появляется ошибка `FirebaseApp.androidlib`, удалить `Library/SourceAssetDB` и перезапустить Unity.
- Unity Editor требует JAVA_HOME → указать на OpenJDK из Unity Hub (уже настроено).

## Архитектура

Слоистая, без DI-контейнеров:

- **Domain** — чистый C#, не зависит от UnityEngine. Содержит `MachineModel`, `EconomyService`, `BoostService`, `CostCalculator`, `OfflineProgressCalculator`.
- **Services** — за интерфейсами: `IStorageService`, `IAnalyticsProvider`, `IIAPService`. Реализации скрыты от игровой логики.
- **Presentation** — тонкие MonoBehaviour-адаптеры: `BalanceView`, `FactoryView`, `MachineView`, `BoostView`, `IAPView`.
- **Config** — `GameConfigSO` (ScriptableObject) реализует `IGameConfig`. Обёрнут в `RemoteConfigProvider` с поддержкой Firebase.
- **Core** — `GameBootstrap.cs` — единственный Composition Root.

**Запрещено и не используется:**
- DI-контейнеры (Zenject, VContainer)
- Синглтоны, GameManager.Instance
- FindObjectOfType
- async void в MonoBehaviour

## Основные системы

- **EconomyService** — баланс, машины, unlock/upgrade, производство. Формулы: `UpgradeCost = BaseUpgradeCost * pow(UpgradeMultiplier, Level)`, `Production = BaseProduction * Level`.
- **BoostService** — временный множитель производства. Учитывает реально прошедшее время, включая сворачивание приложения.
- **OfflineProgressCalculator** — offline-доход с клампом по `MaxOfflineSeconds`.
- **JsonFileStorageService** — сохранение в `Application.persistentDataPath/save.json`.
- **AnalyticsService** — композитный фасад, отправляет события во все провайдеры.
- **RemoteConfigProvider** — обёртка над `GameConfigSO`, подтягивает значения из Firebase Remote Config с fallback на локальные.

## Использованные пакеты / SDK

- `com.unity.purchasing` 5.4.4
- Firebase App 13.17.0
- Firebase Remote Config 13.17.0
- Firebase Analytics 13.17.0
- AppMetrica 6.10.0 (через OpenUPM)
- External Dependency Manager 1.2.187
- TextMeshPro / стандартный Unity UI
- `nunit.framework` через Unity Test Framework

## Тесты

**Всего: 23 (18 EditMode + 5 PlayMode).**

Запуск: `Window → General → Test Runner → EditMode (или PlayMode) → Run All`.

**EditMode (18):**
- CostCalculatorTests (5)
- OfflineProgressCalculatorTests (4)
- EconomyServiceTests (5)
- BoostServiceTests (4)

**PlayMode (5):**
- GameBootstrapPlayModeTests (3): инициализация, offline-доход, Boost при паузе
- EconomyServicePlayModeTests (1)
- MachineViewPlayModeTests (1)

EditMode-тесты не создают MonoBehaviour/GameObject/ScriptableObject. PlayMode-тесты используют фейковые реализации сервисов.

## Профилирование Android build

Проведено на реальном устройстве **realme RMX5078** через Unity Profiler (Development Build + Autoconnect Profiler).

**Метрики:**
- FPS: стабильно ~60, пики до 30 FPS при загрузке сцены и первого рендера
- CPU Main Thread: ~16 мс (соответствует 60 FPS)
- Рендеринг: URP, батчинг работает корректно
- GC Alloc: в разумных пределах, критичных всплесков нет

**Размер APK:** ~60 МБ. Включает нативные Firebase-плагины для всех платформ (Windows .dll, Linux .so, macOS .bundle). При релизной сборке можно исключить нецелевые платформы и сократить размер до ~25 МБ.

**Время сборки:** 9 минут 17 секунд (первая полная сборка с IL2CPP).

**Выводы:** производительность в пределах нормы для мобильной idle-игры. Оптимизация не требуется.

## GitFlow

- `main` — финальное состояние проекта.
- `develop` — ветка разработки.
- Conventional Commits: `feat`, `fix`, `chore`, `docs`, `test`, `refactor`.
- Git LFS для больших бинарников Firebase (`.dll`, `.so`, `.bundle`, `.aar`).

## Известные ограничения

- **AppMetrica API key** оставлен заглушкой `REPLACE_WITH_APP_METRICA_API_KEY` из-за временной недоступности сервиса AppMetrica. `AppMetricaProvider` реализует **graceful degradation**: при невалидном ключе SDK не активируется, приложение продолжает работу, события пишутся в `Debug.Log` с пометкой `[AppMetrica/Degraded]`. При замене ключа AppMetrica активируется автоматически.
- **Unity IAP Fake Store** работает только в Unity Editor. На реальном Android-устройстве покупка не завершается, так как продукт `coins_pack_small` не создан в Google Play Console (не требуется по ТЗ).
- **Firebase Analytics** данные появляются в GA4 с задержкой ~24 часа.
- **Firebase SDK** выводит warning `Database URL not set in the Firebase config` при инициализации — это ожидаемое поведение, проект не использует Realtime Database.
- Из репозитория исключены нативные Firebase-плагины для macOS (`.bundle`) — они подтягиваются через Git LFS.
- Async-операции используют `Task` вместо UniTask. Для тестового проекта это оправдано; в production можно рассмотреть UniTask.

## Устройство для проверки

- **Unity Editor:** Windows 11, Unity 6000.3.22f1.
- **Android build:** проверено на realme RMX5078 (Android 12).
- Firebase Remote Config, Firebase Analytics инициализируются на устройстве.
- AppMetrica работает в degraded mode из-за недоступности сервиса.

## Время работы

~18 часов фактической работы, включая настройку окружения (Codex CLI, ProxyAPI, Git LFS, Firebase Console, AppMetrica SDK, EDM4U).

## Что можно улучшить

- Заменить AppMetrica API key на реальный, когда сервис восстановится.
- Создать продукт `coins_pack_small` в Google Play Console для реальной интеграции IAP.
- Добавить Firebase App Check для защиты от несанкционированных запросов.
- Покрыть PlayMode-тестами IAP и Remote Config.
- Исключить нецелевые платформы Firebase из сборки и сократить APK вдвое.
