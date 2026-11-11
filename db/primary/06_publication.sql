\getenv replication_password REPLICATION_PASSWORD

create role replicator with login replication password :'replication_password';
grant select on t1 to replicator;  -- для початкового копіювання

-- зміни публікуються від імені t1, а не партицій: підписник розкладає рядки сам
create publication t1_publication for table t1 with (publish_via_partition_root = true);
