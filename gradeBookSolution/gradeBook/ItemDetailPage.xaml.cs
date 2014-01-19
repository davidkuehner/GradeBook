using gradeBook.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;


namespace gradeBook
{
    /// <summary> DKU
    /// Page affichant une vue d'ensemble d'un objet, ainsi que de ses informations.
    /// </summary>
    public sealed partial class ItemDetailPage : gradeBook.Common.LayoutAwarePage
    {
        /// <summary> DKU
        /// Objet actuellement chargé dans la vue
        /// </summary>
        private GradeDataItem item;

        /// <summary> DKU
        /// Initialisation
        /// </summary>
        public ItemDetailPage()
        {
            this.InitializeComponent();
        }

        /// <summary> Microsoft
        /// Remplit la page à l'aide du contenu passé lors de la navigation. Tout état enregistré est également
        /// fourni lorsqu'une page est recréée à partir d'une session antérieure.
        /// </summary>
        /// <param name="navigationParameter">Valeur de paramètre passée à
        /// <see cref="Frame.Navigate(Type, Object)"/> lors de la requête initiale de cette page.
        /// </param>
        /// <param name="pageState">Dictionnaire d'état conservé par cette page durant une session
        /// antérieure. Null lors de la première visite de la page.</param>
        protected override void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {
            // Autorise l'état de page enregistré à substituer l'élément initial à afficher
            if (pageState != null && pageState.ContainsKey("SelectedItem"))
            {
                navigationParameter = pageState["SelectedItem"];
            }

            // TODO: créez un modèle de données approprié pour le domaine posant problème pour remplacer les exemples de données
            //var item = GradeDataSource.GetItem((String)navigationParameter);

            item = (GradeDataItem)navigationParameter;

            this.DefaultViewModel["Group"] = item.Group;
            this.DefaultViewModel["Items"] = item.Group.Items;
            this.flipView.SelectedItem = item;
        }

        /// <summary> Microsoft
        /// Conserve l'état associé à cette page en cas de suspension de l'application ou de la
        /// suppression de la page du cache de navigation. Les valeurs doivent être conformes aux
        /// exigences en matière de sérialisation de <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="pageState">Dictionnaire vide à remplir à l'aide de l'état sérialisable.</param>
        protected override void SaveState(Dictionary<String, Object> pageState)
        {
            var selectedItem = (GradeDataCommon)this.flipView.SelectedItem;
            pageState["SelectedItem"] = selectedItem.Id;
        }

        /// <summary> DKU
        /// Handler pour la supression de l'objet courant
        /// </summary>
        /// <param name="sender">Bouton source</param>
        /// <param name="e"></param>
        void DeleteItem(object sender, RoutedEventArgs e)
        {
            deleteItemButton.IsEnabled = false;
            editItemButton.IsEnabled = false;
            item.databaseDelete();
        }

        /// <summary> DKU
        /// Lorsque la grille enfant de la popup d'édition est chargée,
        /// centrage de la popup au milieu de la fenêtre.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void gridChild_Loaded(object sender, RoutedEventArgs e)
        {
            editPopup.Width = Window.Current.Bounds.Width;
            editPopup.HorizontalOffset = (Window.Current.Bounds.Width - gridChild.ActualWidth) / 2;
            editPopup.VerticalOffset = (Window.Current.Bounds.Height - gridChild.ActualHeight) / 2;
        }

        /// <summary> DKU
        /// Handler pour l'édition de l'objet.
        /// Empèche tout autre action lors de l'édition.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void editItemButton_Click(object sender, RoutedEventArgs e)
        {
            editPopup.IsOpen = true;
            flipView.IsEnabled = false;
            deleteItemButton.IsEnabled = false;
            editItemButton.IsEnabled = false;
            backButton.IsEnabled = false;
        }

        /// <summary> DKU
        /// Permet de charger les information dans les champs d'édition.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void editPopup_Opened(object sender, object e)
        {
            InputTitle.Text = item.Title;
            InputDescription.Document.SetText(Windows.UI.Text.TextSetOptions.None, item.Description);
            InputPonderation.Text = item.Ponderation.ToString();
            InputGrade.Text = item.Grade.ToString();
        }

        /// <summary> DKU
        /// Handler pour le bouton de sauvegarde.
        /// Sauve les modification et redonne accès au fonctionnalités.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            editPopup.IsOpen = false;
            flipView.IsEnabled = true;
            deleteItemButton.IsEnabled = true;
            editItemButton.IsEnabled = true;
            backButton.IsEnabled = true;

            
            try
            {
                string strPond = InputPonderation.Text;
                item.Ponderation = Double.Parse(strPond);
                string strGrade = InputGrade.Text;
                item.Grade = Double.Parse(strGrade);
            }
            catch
            {
                item.Grade = 0.0;
                item.Ponderation = 0.0;
            }
            
            item.Title = InputTitle.Text;

            string strDesc = string.Empty;
            InputDescription.Document.GetText(Windows.UI.Text.TextGetOptions.AdjustCrlf, out strDesc);
            item.Description = strDesc;

            item.databaseUpdate();
        }

        /// <summary> DKU
        /// Handler pour le bouton cancel.
        /// Redonne accès aux fonctionnalités
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            editPopup.IsOpen = false;
            flipView.IsEnabled = true;
            deleteItemButton.IsEnabled = true;
            editItemButton.IsEnabled = true;
            backButton.IsEnabled = true;
        }
    }
}
