using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.Eskhata.Order;

namespace litiko.Eskhata.Server
{
  partial class OrderFunctions
  {
    /// <summary>
    /// Создать простой документ.
    /// </summary>
    /// <returns>Простой документ.</returns>
    [Remote]
    public static Sungero.Docflow.ISimpleDocument CreateSimpleDocument()
    {
      return Sungero.Docflow.SimpleDocuments.Create();
    }
    
    /// <summary>
    /// Создать приказ.
    /// </summary>
    /// <returns>Приказ.</returns>
    [Remote, Public]
    public static litiko.Eskhata.IOrder CreateOrder()
    {
      return litiko.Eskhata.Orders.Create();
    }    
  }
}