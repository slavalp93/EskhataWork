using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace litiko.RegulatoryDocuments.Client
{
  public class ModuleFunctions
  {

    /// <summary>
    /// 
    /// </summary>
    public virtual void Function4()
    {
      Dialogs.NotifyMessage("В разработке...");
    }

    /// <summary>
    /// Создать нормативный документ
    /// </summary>
    public virtual void CreateRegulatoryDocument()
    {
      litiko.RegulatoryDocuments.PublicFunctions.RegulatoryDocument.Remote.CreateRegulatoryDocument().Show();
    }
    
    /// <summary>
    /// Диалог для создания версии из файла.
    /// </summary>
    /// <param name="document">Документ, для которого создается версия.</param>
    /// <param name="versionNote">Примечание к создаваемой версии.</param>
    public virtual void CreateFromFileDialog(Sungero.Docflow.IOfficialDocument document, string versionNote)
    {
      var dialog = Dialogs.CreateInputDialog(Resources.CreateVersionFromFileDiaolog_Tittle);
      dialog.Width = 500;
      var fileSelector = dialog.AddFileSelect(Resources.CreateVersionFromFileDiaolog_File, true);
                       
      var btnOk = dialog.Buttons.AddOk();
      var btnCancel = dialog.Buttons.AddCancel();
      dialog.SetOnButtonClick(b =>
                              {
                                if (b.Button == btnOk && b.IsValid)
                                {
                                  try
                                  {
                                    var fileContent = fileSelector.Value.Content;
                                    var fileName = fileSelector.Value.Name;
                                    using (var memory = new System.IO.MemoryStream(fileContent))
                                    {
                                      var ext = System.IO.Path.GetExtension(fileName).TrimStart('.').ToLower();
                                      var version = document.CreateVersionFrom(memory, ext);
                                      version.Note = versionNote;
                                      document.Save();                                      
                                      Dialogs.NotifyMessage(litiko.Eskhata.Resources.VersionCreatedSuccessfully);
                                    }
                                  }
                                  catch (AppliedCodeException ae)
                                  {
                                    b.AddError(ae.Message, fileSelector);
                                  }
                                  catch (Exception ex)
                                  {                                    
                                    Logger.Error(ex.Message, ex);
                                    b.AddError(ex.Message, fileSelector);
                                  }
                                }
                              });
      dialog.Show();      
    }

  }
}