-- 1000 клієнтів, у кожного один рахунок.
create function t1_random_message()
returns jsonb
language sql
volatile
as $$
    select jsonb_build_object(
               'account_number', 'UA' || lpad(client_id::text, 27, '0'),
               'client_id',      client_id,
               'operation_type', case when random() < 0.5 then 'online' else 'offline' end
           )
    from (select 1 + floor(random() * 1000)::int as client_id) as client;
$$;

-- Оброблені операції, рівномірно розкидані по останньому p_period.
create procedure t1_generate(p_rows integer default 100000, p_period interval default '4 months')
language plpgsql
as $$
declare
    v_to   timestamptz := now();
    v_from timestamptz := v_to - p_period;
begin
    perform t1_create_partitions(v_from, v_to);

    insert into t1 (operation_date, amount, state, message)
    select v_from + random() * (v_to - v_from) as operation_date,
           round((1 + random() * 9999)::numeric, 2),
           1,
           t1_random_message()
    from generate_series(1, p_rows)
    order by operation_date;  -- щоб id зростав разом із датою

    -- тригер на вставку не реагує
    refresh materialized view t1_client_totals;
end;
$$;

-- Нова операція (state = 0) з випадковими даними.
create function t1_add_operation()
returns uuid
language sql
as $$
    insert into t1 (amount, state, message)
    values (round((1 + random() * 9999)::numeric, 2), 0, t1_random_message())
    returning operation_guid;
$$;

-- На парній секунді обробляє парні id, на непарній — непарні.
create function t1_process_operations()
returns bigint
language sql
as $$
    with processed as (
        update t1
        set state = 1
        where state = 0
          and id % 2 = floor(extract(second from now()))::int % 2
        returning 1
    )
    select count(*) from processed;
$$;
