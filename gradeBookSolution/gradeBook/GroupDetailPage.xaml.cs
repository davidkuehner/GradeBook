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
    /// Page affichant une vue d'ensemble d'un groupe, ainsi qu'un aperçu des éléments
    /// qu'il contient.
    /// </summary>
    public sealed partial class GroupDetailPage : gradeBook.Common.LayoutAwarePage
    {
        /// <summary> DKU
        /// Groupe actuellement chargé dans la vue.
        /// </summary>
        private GradeDataGroup group;

        /// <summary> DKU
        /// Initialisation
        /// </summary>
        public GroupDetailPage()
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
            group = (GradeDataGroup)navigationParameter;

            // Adaptation afin de faire correspondre les nouvelles classes DKU
            this.DefaultViewModel["Group"] = group;
            this.DefaultViewModel["Items"] = group.Items;
        }

        /// <summary> Microsoft
        /// Invoqué lorsqu'un utilisateur clique sur un élément.
        /// </summary>
        /// <param name="sender">GridView (ou ListView lorsque l'état d'affichage de l'application est Snapped)
        /// affichant l'élément sur lequel l'utilisateur a cliqué.</param>
        /// <param name="e">Données d'événement décrivant l'élément sur lequel l'utilisateur a cliqué.</param>
        void ItemView_ItemClick(object sender, ItemClickEventArgs e)
        {
            // Accédez à la page de destination souhaitée, puis configurez la nouvelle page
            // en transmettant les informations requises en tant que paramètre de navigation.
            if (e.ClickedItem.GetType() == typeof(GradeDataGroup))
            {
                var item = ((GradeDataGroup)e.ClickedItem);
                this.Frame.Navigate(typeof(GroupDetailPage), item);
            }
            else if (e.ClickedItem.GetType() == typeof(GradeDataItem))
            {
                var item = ((GradeDataItem)e.ClickedItem);
                this.Frame.Navigate(typeof(ItemDetailPage), item);
            }
            else
            {
                throw new ArgumentException("Type of item click event not handled.");
            }

            
        }

        /// <summary> DKU
        /// Handler pour la création d'un nouveau group enfant
        /// </summary>
        /// <param name="sender">Bouton source</param>
        /// <param name="e"></param>
        void NewGroup(object sender, RoutedEventArgs e)
        {
            group.databaseAppendNewGroup();
        }

        /// <summary> DKU
        /// Handler pour la création d'un nouvel objet enfant
        /// </summary>
        /// <param name="sender">Bouton source</param>
        /// <param name="e"></param>
        void NewItem(object sender, RoutedEventArgs e)
        {
            group.databaseAppendNewItem();
        }

        /// <summary> DKU
        /// Handler pour la supression du groupe courant
        /// </summary>
        /// <param name="sender">Bouton source</param>
        /// <param name="e"></param>
        void DeleteGroup(object sender, RoutedEventArgs e)
        {
            deleteGroupButton.IsEnabled = false;
            editGroupButton.IsEnabled = false;
            newItemButton.IsEnabled = false;
            newGroupButton.IsEnabled = false;
            group.databaseDelete();

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
        /// Handler pour l'édition du groupe.
        /// Empèche tout autre action lors de l'édition.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void editGroupButton_Click(object sender, RoutedEventArgs e)
        {
            editPopup.IsOpen = true;
            itemGridView.IsEnabled = false;
            deleteGroupButton.IsEnabled = false;
            newItemButton.IsEnabled = false;
            newGroupButton.IsEnabled = false;
            editGroupButton.IsEnabled = false;
            backButton.IsEnabled = false;
        }

        /// <summary> DKU
        /// Permet de charger les information dans les champs d'édition.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void editPopup_Opened(object sender, object e)
        {
            InputTitle.Text = group.Title;
            InputDescription.Document.SetText(Windows.UI.Text.TextSetOptions.None, group.Description);
            InputPonderation.Text = group.Ponderation.ToString();
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
            itemGridView.IsEnabled = true;
            deleteGroupButton.IsEnabled = true;
            newItemButton.IsEnabled = true;
            newGroupButton.IsEnabled = true;
            editGroupButton.IsEnabled = true;
            backButton.IsEnabled = true;

            try
            {
                string strPond = InputPonderation.Text;
                group.Ponderation = Double.Parse(strPond);

            }
            catch
            {
                group.Ponderation = 0.0;
            }

            group.Title = InputTitle.Text;

            string strDesc = string.Empty;
            InputDescription.Document.GetText(Windows.UI.Text.TextGetOptions.AdjustCrlf, out strDesc);
            group.Description = strDesc;

            group.databaseUpdate();
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
            itemGridView.IsEnabled = true;
            deleteGroupButton.IsEnabled = true;
            newItemButton.IsEnabled = true;
            newGroupButton.IsEnabled = true;
            editGroupButton.IsEnabled = true;
            backButton.IsEnabled = true;
        }
    }
}
