# Использование AI

## Какие AI-инструменты использовались

- Codex CLI — модель `openai/gpt-5.6-luna` через ProxyAPI.
- Qwen Code — использовался на начальных этапах; от него остались каркас
  папок и базовые Domain-классы.

## Какие задачи передавались AI

- Генерация Domain-классов, Services, UI, тестов и документации.
- Ревизия и исправление кода, сгенерированного другим AI.
- Интеграция Unity IAP v5. Codex проверил актуальные сигнатуры установленного
  пакета `com.unity.purchasing` 5.4.4 в локальном PackageCache.

## Ключевые промпты

- «Реализуй Domain-слой Mini Factory по слоистой архитектуре».
- «Проверь и исправь offline-прогресс и работу Boost при паузе приложения».
- «Интегрируй Unity IAP v5 через event-driven StoreController и Fake Store».
- «Создай EditMode-тесты для стоимости, экономики, offline-дохода и Boost».

## Что реализовано с помощью AI

С помощью AI реализована почти вся кодовая база:

- Domain-слой и экономические расчёты.
- Config-слой.
- Storage, Analytics и IAP-сервисы.
- Presentation/UI-компоненты.
- Persistence и `GameBootstrap`.
- EditMode-тесты.
- README и AI_USAGE.

## Что сделано самостоятельно

- Настройка окружения: Node.js, Codex CLI, ProxyAPI, Git и VPN.
- Ручная сборка UI-сцены в Unity Editor.
- Создание и настройка `GameConfig.asset`.
- Ручное подключение TextMeshPro и Unity IAP 5 через Package Manager.
- Финальный review и проверка решений AI.

## Изменённые или отклонённые предложения AI

- Первоначально предлагался старый API IAP v4:
  `ConfigurationBuilder` и `IStoreListener`. Он отклонён и заменён на v5
  `StoreController`.
- Предложение использовать DI-контейнер отклонено из-за масштаба проекта и
  требования не добавлять необоснованный overengineering.
- AI добавил метод-алиас TickProduction для тестов — было решено оставить только TryTickProduction 
  как единый публичный метод для обновления экономики.

## Ошибки, обнаруженные в AI-коде

- Qwen ошибочно рассчитывал производство машины от стоимости улучшения.
  Исправлено на расчёт от `BaseProduction`.
- Qwen использовал формулу upgrade со смещением `level - 1`. Исправлено на
  формулу с текущим `level`.
- Codex первоначально использовал `GetProductById`. В Unity IAP 5.4.4 этот
  метод отсутствует; заменён на `GetProductByCatalogListingId`.
- Codex первоначально не подписался на `OnPurchaseConfirmed` и
  `OnPurchaseDeferred`. Подписки добавлены после проверки исходников пакета.
- Обнаружена ошибка lifecycle Boost: при возврате из паузы не учитывалось
  прошедшее время. В `GameBootstrap` добавлен вызов `BoostService.Tick` за
  время сворачивания приложения.
