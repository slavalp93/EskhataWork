DO $$
BEGIN
  CREATE TABLE IF NOT EXISTS Sungero_Docflow_ApiKeys
  (
    Key citext NOT NULL,
    Value citext,
    PRIMARY KEY (Key)
  );

  IF NOT EXISTS(SELECT 1 FROM Sungero_Docflow_ApiKeys WHERE Key = 'ApiKey')
  THEN
    INSERT INTO Sungero_Docflow_ApiKeys(Key, Value) VALUES ('ApiKey', '');
  END IF;
END$$