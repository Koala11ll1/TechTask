-- Сума оброблених операцій за клієнтом і типом операції.
-- Оновлюється, коли рядок t1 переходить зі стану 0 в 1.

create materialized view t1_client_totals as
select (message ->> 'client_id')::bigint as client_id,
       message ->> 'operation_type'      as operation_type,
       sum(amount)                       as total_amount
from t1
where state = 1
group by 1, 2;

-- потрібен для refresh concurrently
create unique index t1_client_totals_pkey on t1_client_totals (client_id, operation_type);

create function t1_refresh_client_totals()
returns trigger
language plpgsql
as $$
begin
    if exists (
        select
        from old_rows o
        join new_rows n using (id)
        where o.state = 0 and n.state = 1
    ) then
        refresh materialized view concurrently t1_client_totals;
    end if;

    return null;
end;
$$;

-- Один refresh на оператор, а не на рядок. "update of state" тут не можна:
-- перехідні таблиці не працюють зі списком колонок.
create trigger t1_state_processed
    after update on t1
    referencing old table as old_rows new table as new_rows
    for each statement execute function t1_refresh_client_totals();
