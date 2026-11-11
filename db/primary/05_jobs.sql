select cron.schedule('t1-add-operation',      '5 seconds', 'select t1_add_operation()');
select cron.schedule('t1-process-operations', '3 seconds', 'select t1_process_operations()');

-- інакше журнал запусків росте на ~30 тис. рядків за добу
select cron.schedule(
    'cron-purge-run-details',
    '0 * * * *',
    $$delete from cron.job_run_details where end_time < now() - interval '1 day'$$
);
