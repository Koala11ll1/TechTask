-- Унікальність operation_guid.
-- Унікальний індекс на t1 мусив би містити operation_date, тобто той самий GUID
-- пройшов би з іншою датою. Тому GUID-и дублюються в окрему таблицю з PK.

create table t1_operation_guid (
    operation_guid uuid primary key
);

create function t1_sync_operation_guid()
returns trigger
language plpgsql
as $$
begin
    if tg_op in ('DELETE', 'UPDATE') then
        delete from t1_operation_guid where operation_guid = old.operation_guid;
    end if;

    if tg_op in ('INSERT', 'UPDATE') then
        insert into t1_operation_guid (operation_guid) values (new.operation_guid);
    end if;

    return null;
end;
$$;

-- Саме after: перенесення рядка між партиціями — це delete + insert,
-- і before insert спрацював би раніше, ніж старий GUID буде видалено.
create trigger t1_guid_insert
    after insert on t1
    for each row execute function t1_sync_operation_guid();

create trigger t1_guid_update
    after update of operation_guid on t1
    for each row
    when (old.operation_guid is distinct from new.operation_guid)
    execute function t1_sync_operation_guid();

create trigger t1_guid_delete
    after delete on t1
    for each row execute function t1_sync_operation_guid();

create function t1_truncate_operation_guid()
returns trigger
language plpgsql
as $$
begin
    truncate t1_operation_guid;
    return null;
end;
$$;

create trigger t1_guid_truncate
    after truncate on t1
    for each statement execute function t1_truncate_operation_guid();
