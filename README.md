# Mini Factory (тестовое задание Unity Developer)

## Что это

Небольшой мобильный Unity-проект с производственной фабрикой, экономикой,
временными Boost, offline-прогрессом, сохранением, Unity IAP и аналитикой.

## Требования

- Unity 6000.3.22f1
- Android

## Как открыть

Открыть проект в Unity Hub и дождаться импорта пакетов.

## Как запустить

Открыть сцену `Assets/_Project/Scenes/Main.unity` и нажать `Play`.

В текущем рабочем дереве эта сцена не обнаружена, поэтому перед запуском нужно
проверить её наличие или создать/подключить актуальную сцену в Unity Editor.

## Как собрать Android build

1. Открыть `File → Build Settings`.
2. Выбрать `Android`.
3. Нажать `Switch Platform`.
4. Нажать `Build`.

## Архитектура

Проект использует слоистую архитектуру:

- `Domain` — чистый C# без зависимости от Unity.
- `Services` — Storage, Analytics и IAP за интерфейсами.
- `Presentation` — тонкие `MonoBehaviour`-адаптеры UI.
- `Config` — конфигурация на базе `ScriptableObject` и интерфейсов Domain.

`GameBootstrap.cs` — Composition Root и единственное место, где создаются и
связываются основные системы.

DI-контейнеры, синглтоны и `GameManager.Instance` не используются.

## Основные системы

- `EconomyService` — баланс, машины, открытие, улучшения и производство.
- `BoostService` — временное ускорение производства и его lifecycle.
- `OfflineProgressCalculator` — расчёт дохода за время отсутствия с лимитом.
- `IStorageService` — абстракция сохранения; текущая реализация использует JSON.
- `IAnalyticsProvider` — абстракция аналитики; подключён Console-провайдер.
- `IIAPService` — абстракция покупок; подключена Unity IAP 5.4.4 с Fake Store.

## Использованные packages/SDK

- `com.unity.purchasing` 5.4.4
- TextMeshPro / стандартный Unity UI
- `nunit.framework` через Unity Test Framework

## Тесты

Добавлено 18 EditMode-тестов для стоимости, offline-дохода, экономики и Boost.
Дополнительно добавлены PlayMode-тесты для `GameBootstrap`, экономики и UI.

Запуск:

`Window → General → Test Runner → EditMode → Run All`

Тесты не создают `MonoBehaviour`, `GameObject` или `ScriptableObject`.

## GitFlow

Работа велась в `develop`, финальное состояние предназначено для `main`.

## Известные ограничения

- Async-операции используют `Task` вместо UniTask. Для тестового проекта это
  оправдано; в production можно рассмотреть UniTask.
- Firebase Remote Config не подключён.
- AppMetrica/Firebase Analytics не подключены; используется Console-провайдер.
- Профилирование Android build не проводилось.
- Сцену `Assets/_Project/Scenes/Main.unity` необходимо проверить в Unity Editor:
  в рабочем дереве она отсутствует.

## Устройство и проверка

Проверка проводилась в Unity Editor на Windows.

Android build на реальном устройстве не тестировался.

## Фактическое время работы

Примерно 3–4 часа, включая настройку окружения, разработку, ревизию кода,
настройку Unity-пакетов и документацию.

## Что осталось незавершённым

- Firebase Remote Config.
- Реальный Analytics Provider.
- Финальная проверка/подключение сцены `Main.unity`.

## Следующий шаг

Подключить Firebase Remote Config с fallback на локальный `GameConfigSO`,
добавить AppMetrica как второго Analytics Provider и написать PlayMode-тесты
для `GameBootstrap`.
