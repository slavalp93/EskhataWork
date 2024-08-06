using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using litiko.Eskhata.OutgoingDocumentBase;

namespace litiko.Eskhata.Client
{
  partial class OutgoingDocumentBaseFunctions
  {
    public static void ShowSelectEnvelopeFormatDialog(List<IOutgoingDocumentBase> outgoingDocuments, List<Sungero.Docflow.IContractualDocumentBase> contractualDocuments, List<Sungero.Docflow.IAccountingDocumentBase> accountingDocuments)
    {
      if (outgoingDocuments == null)
        outgoingDocuments = new List<IOutgoingDocumentBase>();
      
      if (contractualDocuments == null)
        contractualDocuments = new List<Sungero.Docflow.IContractualDocumentBase>();
      
      if (accountingDocuments == null)
        accountingDocuments = new List<Sungero.Docflow.IAccountingDocumentBase>();
      
      var resources = OutgoingDocumentBases.Resources;
      var defaultEnvelopeFormat = resources.DLEnvelope.ToString();
      var defaultPrintSender = true;
      
      // Из персональных настроек взять формат конверта и необходимость печати отправителя.
      var personalSetting = Sungero.Docflow.PublicFunctions.PersonalSetting.GetPersonalSettings(Sungero.Company.Employees.Current);
      if (personalSetting != null)
      {
        defaultEnvelopeFormat = personalSetting.EnvelopeFormat.HasValue ?
          Sungero.Docflow.PersonalSettings.Info.Properties.EnvelopeFormat.GetLocalizedValue(personalSetting.EnvelopeFormat.Value) :
          resources.C4Envelope.ToString();
        defaultPrintSender = personalSetting.PrintSender ?? true;
      }
      
      // Диалог выбора отчета.
      var dialog = Dialogs.CreateInputDialog(resources.EnvelopePrinting);
      dialog.HelpCode = Constants.Docflow.OutgoingDocumentBase.EnvelopeDialogHelpCode;
      dialog.Buttons.AddOkCancel();
      var envelopeFormat = dialog.AddSelect(resources.EnvelopeFormat, true, defaultEnvelopeFormat)
        .From(resources.C4Envelope, litiko.Eskhata.OutgoingDocumentBases.Resources.B4Envelope);
      var needPrintSender = dialog.AddBoolean(resources.NeedPrintSender, defaultPrintSender);
      
      if (dialog.Show() != DialogButtons.Ok)
        return;
      
      // Выбрать отчет в зависимости от указанного формата.
      if (envelopeFormat.Value == resources.C4Envelope)
      {
        var report = Sungero.Docflow.Reports.GetEnvelopeC4Report();
        report.PrintSender = needPrintSender.Value;
        report.OutgoingDocuments.AddRange(outgoingDocuments);
        report.ContractualDocuments.AddRange(contractualDocuments);
        report.AccountingDocuments.AddRange(accountingDocuments);
        report.Open();
      }
      else if (envelopeFormat.Value == litiko.Eskhata.OutgoingDocumentBases.Resources.B4Envelope)
      {
        var report = DocflowEskhata.Reports.GetEnvelopeB4Report();
        report.PrintSender = needPrintSender.Value;
        report.OutgoingDocuments.AddRange(outgoingDocuments);
        report.ContractualDocuments.AddRange(contractualDocuments);
        report.AccountingDocuments.AddRange(accountingDocuments);
        report.Open();
      }
    }
  }
}