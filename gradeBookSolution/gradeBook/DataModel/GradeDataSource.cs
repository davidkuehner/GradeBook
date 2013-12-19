using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.ApplicationModel.Resources.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using System.Collections.Specialized;

// Le modèle de données défini par ce fichier sert d'exemple représentatif d'un modèle fortement typé
// prenant en charge les notifications lorsque les membres sont ajoutés, supprimés ou modifiés. Les noms
// de propriétés choisis correspondent aux liaisons de données dans les modèles d'élément standard.
//
// Les applications peuvent utiliser ce modèle comme point de départ et le modifier à leur convenance, ou le supprimer complètement et
// le remplacer par un autre correspondant à leurs besoins.

namespace gradeBook.Data
{
    /// <summary>
    /// Classe de base pour <see cref="GradeDataItem"/> et <see cref="GradeDataGroup"/> qui
    /// définit les propriétés communes au deux.
    /// </summary>
    [Windows.Foundation.Metadata.WebHostHidden]
    public abstract class GradeDataCommon : gradeBook.Common.BindableBase
    {
        private static Uri _baseUri = new Uri("ms-appx:///");

        public GradeDataCommon(String uniqueId, String title, Double ponderation, GradeDataGroup group)
        {
            this._uniqueId = uniqueId;
            this._title = title;
            this._ponderation = ponderation;
            this._group = group;
        }

        private string _uniqueId = string.Empty;
        public string UniqueId
        {
            get { return this._uniqueId; }
            set { this.SetProperty(ref this._uniqueId, value); }
        }

        private string _title = string.Empty;
        public string Title
        {
            get { return this._title; }
            set { this.SetProperty(ref this._title, value); }
        }

        private Double _ponderation = 0.0;
        public Double Ponderation
        {
            get { return this._ponderation; }
            set { this.SetProperty(ref this._ponderation, value); }
        }

        private GradeDataGroup _group;
        public GradeDataGroup Group
        {
            get { return this._group; }
            set { this.SetProperty(ref this._group, value); }
        }

        public override string ToString()
        {
            return this.Title;
        }
    }

    /// <summary>
    /// Modèle de données d'élément générique.
    /// </summary>
    public class GradeDataItem : GradeDataCommon
    {
        public GradeDataItem(String uniqueId, String title, Double ponderation, GradeDataGroup group, Double grade)
            : base(uniqueId, title, ponderation, group)
        {
            this._grade = grade;
        }
        private Double _grade = 0.0;
        public Double Grade
        {
            get { return this._grade; }
            set { this.SetProperty(ref this._grade, value); }
        }
        
    }

    /// <summary>
    /// Modèle de données de groupe générique.
    /// </summary>
    public class GradeDataGroup : GradeDataCommon
    {
        public GradeDataGroup(String uniqueId, String title, Double ponderation, GradeDataGroup group)
            : base(uniqueId, title, ponderation, group)
        {
            Items.CollectionChanged += ItemsCollectionChanged;
        }

        private void ItemsCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            // Fournit un sous-ensemble de la collection complète d'éléments avec laquelle effectuer une liaison à partir d'un GroupedItemsPage
            // pour deux raisons : GridView ne virtualise pas les collections d'éléments volumineuses, et
            // améliore l'expérience utilisateur lors de la navigation dans des groupes contenant un nombre important
            // d'éléments.
            //
            // 12 éléments maximum sont affichés, car cela se traduit par des colonnes de grille remplies,
            // qu'il y ait 1, 2, 3, 4 ou 6 lignes affichées

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (e.NewStartingIndex < 12)
                    {
                        TopItems.Insert(e.NewStartingIndex,Items[e.NewStartingIndex]);
                        if (TopItems.Count > 12)
                        {
                            TopItems.RemoveAt(12);
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Move:
                    if (e.OldStartingIndex < 12 && e.NewStartingIndex < 12)
                    {
                        TopItems.Move(e.OldStartingIndex, e.NewStartingIndex);
                    }
                    else if (e.OldStartingIndex < 12)
                    {
                        TopItems.RemoveAt(e.OldStartingIndex);
                        TopItems.Add(Items[11]);
                    }
                    else if (e.NewStartingIndex < 12)
                    {
                        TopItems.Insert(e.NewStartingIndex, Items[e.NewStartingIndex]);
                        TopItems.RemoveAt(12);
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    if (e.OldStartingIndex < 12)
                    {
                        TopItems.RemoveAt(e.OldStartingIndex);
                        if (Items.Count >= 12)
                        {
                            TopItems.Add(Items[11]);
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    if (e.OldStartingIndex < 12)
                    {
                        TopItems[e.OldStartingIndex] = Items[e.OldStartingIndex];
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    TopItems.Clear();
                    while (TopItems.Count < Items.Count && TopItems.Count < 12)
                    {
                        TopItems.Add(Items[TopItems.Count]);
                    }
                    break;
            }
        }

        private ObservableCollection<GradeDataCommon> _items = new ObservableCollection<GradeDataCommon>();
        public ObservableCollection<GradeDataCommon> Items
        {
            get { return this._items; }
        }

        private ObservableCollection<GradeDataCommon> _topItem = new ObservableCollection<GradeDataCommon>();
        public ObservableCollection<GradeDataCommon> TopItems
        {
            get {return this._topItem; }
        }
    }

    /// <summary>
    /// Crée une collection de groupes et d'éléments dont le contenu est codé en dur.
    /// 
    /// GradeDataSource initialise avec les données des espaces réservés plutôt que les données de production
    /// actives, afin que les exemples de données soient fournis à la fois au moment de la conception et de l'exécution.
    /// </summary>
    public sealed class GradeDataSource
    {
        private static GradeDataSource _GradeDataSource = new GradeDataSource();

        // TODO david -> change type of RootGroup attribute to group
        private ObservableCollection<GradeDataGroup> _rootGroup = new ObservableCollection<GradeDataGroup>();
        public ObservableCollection<GradeDataGroup> RootGroup
        {
            get { return this._rootGroup; }
        }

        /*public static IEnumerable<GradeDataGroup> GetGroups(string uniqueId)
        {
            if (!uniqueId.Equals("RootGroup")) throw new ArgumentException("Only 'RootGroup' is supported as a collection of groups");
            
            return _GradeDataSource.RootGroup;
        }*/

        public IEnumerable<GradeDataGroup> GetRootGroup()
        {
            return _GradeDataSource.RootGroup;
        }

        /*public static GradeDataGroup GetGroup(string uniqueId)
        {
            // Une simple recherche linéaire est acceptable pour les petits groupes de données
            var matches = _GradeDataSource.RootGroup.Where((group) => group.UniqueId.Equals(uniqueId));
            if (matches.Count() == 1) return matches.First();
            return null;
        }*/

        /*public static GradeDataCommon GetItem(string uniqueId)
        {
            // Une simple recherche linéaire est acceptable pour les petits groupes de données
            var matches = _GradeDataSource.RootGroup.SelectMany(group => group.Items).Where((item) => item.UniqueId.Equals(uniqueId));
            if (matches.Count() == 1) return matches.First();
            return null;
        }*/

        public GradeDataSource()
        {
            /// The tree root
            var rootGroup = new GradeDataGroup("RootGroup",
                    "RootGroup",
                    1.0, null);
            this.RootGroup.Add(rootGroup);


            var group1 = new GradeDataGroup("HE-ARC-1",
                    "HE-ARC-1 G",
                    1.0, null);
                    
            group1.Items.Add(new GradeDataItem("P1",
                    "Projet 1 I",
                    1.0,
                    group1,
                    6.0));

            group1.Items.Add(new GradeDataItem("Sciences-A",
                    "Sciences-1 I",
                    1.0,
                    group1,
                    6.0));

            var group11 = new GradeDataGroup("Programmation",
                    "Programmation G",
                    1.0,
                    group1);
            group11.Items.Add(new GradeDataItem("Assembleur",
                    "Assembleur I",
                    1.0,
                    group1,
                    6.0));
            group1.Items.Add(group11);

            rootGroup.Items.Add(group1);

            var group2 = new GradeDataGroup("HE-ARC-2",
                    "HE-ARC-2 G",
                    1.0, null);
            group2.Items.Add(new GradeDataItem("Projet-2",
                    "Projet-2 I",
                    1.0,
                    group2,
                    6.0));
            rootGroup.Items.Add(group2);
        }
    }
}
