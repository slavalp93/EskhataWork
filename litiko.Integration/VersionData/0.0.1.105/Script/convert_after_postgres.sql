UPDATE public.sungero_core_recipient e
SET depcodelitiko_eskhata_litiko = d.code_company_sungero
FROM public.sungero_core_recipient d
WHERE d.id = e.department_company_sungero
AND d.discriminator = '61b1c19f-26e2-49a5-b3d3-0d3618151e12'
AND e.discriminator = 'b7905516-2be5-4931-961c-cb38d5677565'
AND e.depcodelitiko_eskhata_litiko IS NULL;