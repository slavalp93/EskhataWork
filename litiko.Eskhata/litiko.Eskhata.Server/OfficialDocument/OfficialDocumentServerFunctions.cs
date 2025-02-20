using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.Eskhata.OfficialDocument;

namespace litiko.Eskhata.Server
{
  partial class OfficialDocumentFunctions
  {
    /// <summary>
    /// Получить согласующую подпись автора документа.
    /// </summary>
    /// <param name="versionId">Номер версии.</param>
    /// <param name="includeExternalSignature">Признак того, что в выборку включены внешние подписи.</param>
    /// <returns>Электронная подпись.</returns>
    [Public]
    public virtual Sungero.Domain.Shared.ISignature GetEndorsingAuthorSignature(long versionId, bool includeExternalSignature)
    {
      var version = _obj.Versions.FirstOrDefault(x => x.Id == versionId);
      if (version == null)
        return null;
      
      // Только согласующие подписи.
      var versionSignatures = Signatures.Get(version)
        .Where(s => (includeExternalSignature || s.IsExternal != true) && s.SignatureType == SignatureType.Endorsing)
        .ToList();
      if (!versionSignatures.Any())
        return null;
      
      // В приоритете подпись сотрудника из поля "Подготовил". Квалифицированная ЭП приоритетнее простой.
      return versionSignatures
        .OrderByDescending(s => Equals(s.Signatory, _obj.PreparedBy))
        .ThenBy(s => s.SignCertificate == null)
        .ThenByDescending(s => s.SigningDate)
        .FirstOrDefault();
    }
  }
}