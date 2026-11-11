-- Перевірка на primary. Даних не змінює.

\echo '-- партиції та обсяг даних'
select tableoid::regclass       as partition,
       count(*)                 as rows,
       min(operation_date)::date as date_from,
       max(operation_date)::date as date_to
from t1
group by 1
order by 1;

\echo '-- наявний GUID з іншою датою має бути відхилений'
begin;
do $$
begin
    insert into t1 (operation_date, amount, operation_guid, message)
    select now(), amount, operation_guid, message
    from t1
    limit 1;
    raise exception 'дублікат operation_guid було вставлено';
exception
    when unique_violation then
        raise notice 'OK: дублікат відхилено (unique_violation)';
end;
$$;
rollback;

\echo '-- завдання pg_cron'
select j.jobname,
       j.schedule,
       count(*) filter (where d.status = 'succeeded') as succeeded,
       count(*) filter (where d.status = 'failed')    as failed,
       max(d.end_time)                                as last_run
from cron.job j
left join cron.job_run_details d using (jobid)
group by 1, 2
order by 1;

\echo '-- останні операції'
select id, operation_date, amount, state, operation_guid, message
from t1
order by operation_date desc
limit 5;

\echo '-- представлення проти фактичних сум (має бути 0)'
with actual as (
    select (message ->> 'client_id')::bigint as client_id,
           message ->> 'operation_type'      as operation_type,
           sum(amount)                       as total_amount
    from t1
    where state = 1
    group by 1, 2
)
select count(*) as mismatches
from actual
full join t1_client_totals as mv using (client_id, operation_type)
where actual.total_amount is distinct from mv.total_amount;

\echo '-- реплікація; останній запит порівняти з реплікою'
select application_name, state, sent_lsn, replay_lsn from pg_stat_replication;
select count(*) as rows, sum(state) as processed from t1;
