UPDATE sungero_core_recipient
SET code_company_sungero = SUBSTRING(externalcodeli_eskhata_litiko,9,5)
WHERE externalcodeli_eskhata_litiko IS NOT NULL;