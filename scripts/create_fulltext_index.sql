-- Create full-text catalog and full-text index for DynamicFieldsJson
-- Run this against your database (ensure Full-Text feature is enabled on SQL Server).

IF NOT EXISTS (SELECT * FROM sys.fulltext_catalogs WHERE name = N'ProductsFullTextCatalog')
BEGIN
    CREATE FULLTEXT CATALOG ProductsFullTextCatalog AS DEFAULT;
END

-- Drop existing full-text index on Products if exists
IF EXISTS(SELECT * FROM sys.fulltext_indexes fi JOIN sys.objects o ON fi.object_id = o.object_id WHERE o.name = 'Products')
BEGIN
    -- This will drop the full-text index for Products
    DROP FULLTEXT INDEX ON Products;
END

-- Create full-text index on DynamicFieldsJson (requires a unique key index on the table)
-- Ensure there's a unique key index (Id is primary key)

CREATE FULLTEXT INDEX ON dbo.Products(DynamicFieldsJson LANGUAGE 1033)
KEY INDEX PK_Products
ON ProductsFullTextCatalog
WITH CHANGE_TRACKING AUTO;
