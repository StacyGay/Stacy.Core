SELECT 'public '+
		(CASE DATA_TYPE
			WHEN 'varchar' THEN 'string'
			WHEN 'nvarchar' THEN 'string'
			WHEN 'char' THEN 'string'
			WHEN 'text' THEN 'string'
			WHEN 'ntext' THEN 'string'
			WHEN 'tinyint' THEN 'byte'
			WHEN 'bit' THEN 'bool'
			WHEN 'float' THEN 'double'
			WHEN 'datetime' THEN 'DateTime'
			ELSE DATA_TYPE
		END)+
		(CASE WHEN DATA_TYPE NOT IN ('varchar','nvarchar','text','ntext','char') AND IS_NULLABLE = 'YES' THEN '? ' ELSE ' ' END)+
		COLUMN_NAME+' { get; set; }'
  FROM INFORMATION_SCHEMA.COLUMNS c
 WHERE c.table_name = 'tblClientModules' 