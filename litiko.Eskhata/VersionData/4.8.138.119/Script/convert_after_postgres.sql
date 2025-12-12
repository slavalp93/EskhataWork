UPDATE sungero_core_recipient
SET code_company_sungero = SUBSTRING(externalcodeli_eskhata_litiko FROM 9 FOR 5)
WHERE externalcodeli_eskhata_litiko IS NOT NULL;