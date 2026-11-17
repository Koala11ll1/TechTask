create extension if not exists pg_cron;

create table t1 (
    id             bigint         generated always as identity,
    operation_date timestamptz    not null default now(),
    amount         numeric(18, 2) not null,
    state          smallint       not null default 0,
    operation_guid uuid           not null default gen_random_uuid(),
    message        jsonb          not null,

    constraint t1_pkey primary key (id, operation_date),
    constraint t1_state_check check (state in (0, 1)),
    constraint t1_message_check check (
        message ?& array['account_number', 'client_id', 'operation_type']
        and jsonb_typeof(message -> 'account_number') = 'string'
        and jsonb_typeof(message -> 'client_id') = 'number'
        and message ->> 'operation_type' in ('online', 'offline')
    )
) partition by range (operation_date);

create index t1_pending_idx on t1 (id) where state = 0;

create function t1_create_partitions(p_from timestamptz, p_to timestamptz)
returns void
language plpgsql
as $$
declare
    v_month     timestamptz := date_trunc('month', p_from, 'UTC');
    v_partition text;
begin
    while v_month <= p_to loop
        v_partition := 't1_' || to_char(v_month at time zone 'UTC', 'YYYY_MM');

        if to_regclass(v_partition) is null then
            execute format(
                'create table %I partition of t1 for values from (%L) to (%L)',
                v_partition, v_month, v_month + interval '1 month'
            );
        end if;

        v_month := v_month + interval '1 month';
    end loop;
end;
$$;

select t1_create_partitions(now() - interval '4 months', now() + interval '1 month');

select cron.schedule(
    't1-create-partitions',
    '0 3 * * *',
    $$select t1_create_partitions(now(), now() + interval '1 month')$$
);
