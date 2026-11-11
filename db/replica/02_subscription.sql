\getenv primary_host PRIMARY_HOST
\getenv replication_password REPLICATION_PASSWORD

select format('host=%s dbname=%s user=replicator password=%s',
              :'primary_host', current_database(), :'replication_password') as conninfo
\gset

create subscription t1_subscription
    connection :'conninfo'
    publication t1_publication;
