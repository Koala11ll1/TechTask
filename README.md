# Тестове завдання (PostgreSQL)

PostgreSQL 17 і [pg_cron](https://github.com/citusdata/pg_cron) у Docker Compose: `primary` з усім завданням
і `replica`, куди реплікується таблиця `t1`.

## Запуск

```bash
docker compose up -d --build
```

База `bank`, користувач `postgres`. `primary` слухає на 5432, `replica` на 5433
(змінити можна в `.env`, див. `.env.example`). Скрипти з `db/` виконуються під час
першого старту, по порядку номерів. Почати заново: `docker compose down -v`.

Перевірити:

```bash
docker compose exec -T primary psql -U postgres -d bank < db/verify.sql
docker compose exec replica psql -U postgres -d bank -c "select count(*), sum(state) from t1"
```

## Що де

| Пункт | Файл | Суть |
|---|---|---|
| 1 | `01_t1_table.sql` | `t1`, помісячні партиції за `operation_date` |
| 2 | `04_t1_operations.sql`, `07_seed.sql` | `t1_generate()` — 100 000 рядків за чотири місяці |
| 3 | `02_t1_guid_uniqueness.sql` | таблиця `t1_operation_guid` з PK і тригери |
| 4 | `04_t1_operations.sql`, `05_jobs.sql` | `t1_add_operation()` кожні 5 секунд |
| 5 | `04_t1_operations.sql`, `05_jobs.sql` | `t1_process_operations()` кожні 3 секунди |
| 6 | `03_t1_client_totals.sql` | `t1_client_totals` і тригер на `t1` |
| 7 | `06_publication.sql`, `db/replica/` | публікація і підписка |

## Пояснення

**Унікальність GUID.** Унікальний індекс партиціонованої таблиці мусить містити ключ
партиціонування, тому `unique (operation_guid, operation_date)` пропустив би той самий
GUID з іншою датою. GUID-и дублюються в `t1_operation_guid` з первинним ключем,
синхронізацію тримають тригери.

**Матеріалізоване представлення.** Сумуються лише оброблені операції (`state = 1`),
інакше оновлювати його при зміні стану не було б сенсу. Тригер рівня оператора робить
`refresh ... concurrently`, якщо хоча б один рядок перейшов з 0 в 1.

**Реплікація.** Логічна, бо потрібна одна таблиця, а не весь кластер. DDL вона не
переносить, тому `01_t1_table.sql` виконується на обох інстансах, і партиції на
майбутні місяці кожен створює собі сам щоденним завданням.

**Чого завдання не просило.** Щоденне створення партиції на наступний місяць (інакше
вставка впаде першого числа) і щогодинне чищення журналу запусків pg_cron.

## Обмеження

- `update` напряму по партиції, в обхід `t1`, представлення не оновить.
- Перед `t1_generate` з періодом понад чотири місяці ті самі партиції треба створити
  й на репліці: `select t1_create_partitions(...)`.
