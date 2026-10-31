namespace Restaurant.Infrastructure.Persistence;

public static class DatabaseSecuritySql
{
    public const string LockDownPublicSchema = """
        DO $$
        DECLARE r RECORD;
        BEGIN
          FOR r IN SELECT tablename FROM pg_tables WHERE schemaname = 'public' LOOP
            EXECUTE format('ALTER TABLE public.%I ENABLE ROW LEVEL SECURITY', r.tablename);
          END LOOP;
          IF EXISTS (SELECT 1 FROM pg_roles WHERE rolname = 'anon') THEN
            EXECUTE 'REVOKE ALL ON ALL TABLES IN SCHEMA public FROM anon';
            EXECUTE 'REVOKE ALL ON ALL SEQUENCES IN SCHEMA public FROM anon';
            EXECUTE 'REVOKE ALL ON ALL FUNCTIONS IN SCHEMA public FROM anon';
            EXECUTE 'ALTER DEFAULT PRIVILEGES IN SCHEMA public REVOKE ALL ON TABLES FROM anon';
          END IF;
          IF EXISTS (SELECT 1 FROM pg_roles WHERE rolname = 'authenticated') THEN
            EXECUTE 'REVOKE ALL ON ALL TABLES IN SCHEMA public FROM authenticated';
            EXECUTE 'REVOKE ALL ON ALL SEQUENCES IN SCHEMA public FROM authenticated';
            EXECUTE 'REVOKE ALL ON ALL FUNCTIONS IN SCHEMA public FROM authenticated';
            EXECUTE 'ALTER DEFAULT PRIVILEGES IN SCHEMA public REVOKE ALL ON TABLES FROM authenticated';
          END IF;
        END $$;
        """;
}
