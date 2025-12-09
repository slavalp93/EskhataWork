using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.NSI.ResponsibilityMatrix;

namespace litiko.NSI.Server
{
  partial class ResponsibilityMatrixFunctions
  {
    /// <summary>
    /// Получить дубликаты
    /// </summary>
    /// <returns></returns>
    [Remote(IsPure = true)]
    public List<IResponsibilityMatrix> GetDuplicates()
    {
      var selectedCategories = _obj.ContractCategories
                              .Where(cc => cc.Category != null)
                              .Select(cc => cc.Category.Id)
                              .Distinct()
                              .ToList();
    
      var baseQuery = ResponsibilityMatrices.GetAll()
        .Where(x => x.Id != _obj.Id)
        .Where(x => x.Status != Sungero.CoreEntities.DatabookEntry.Status.Closed)
        .Where(x => Equals(x.DocumentKind, _obj.DocumentKind));
    
      List<IResponsibilityMatrix> duplicates;
    
      if (selectedCategories.Count == 0)
      {
        // У текущей записи нет категорий → дубль только те, у кого тоже нет категорий
        duplicates = baseQuery
          .Where(x => !x.ContractCategories.Any())
          .ToList();
      }
      else
      {
        // У текущей записи есть категории → дубль только при пересечении по категориям
        duplicates = baseQuery
          .Where(x => x.ContractCategories
                       .Any(cc => selectedCategories.Contains(cc.Category.Id)))
          .ToList();
      }    
      
      return duplicates;
    }
  }
}