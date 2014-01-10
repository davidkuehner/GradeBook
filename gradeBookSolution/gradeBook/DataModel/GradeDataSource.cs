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
using SQLite;
using System.Diagnostics;

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

        public GradeDataCommon(String title, Double ponderation, GradeDataGroup group, String description)
        {
            this._title = title;
            this._ponderation = ponderation;
            this._group = group;
            this._groupId = group.Id;
            this._description = description;
        }

        /// <summary>
        ///  Constructeur par défaut nécessaire pour SqLite
        /// </summary>
        public GradeDataCommon()
        {
        }

        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

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
        [Ignore]
        public GradeDataGroup Group
        {
            get { return this._group; }
            set
            {
                this.SetProperty(ref this._groupId, value.Id);
                this.SetProperty(ref this._group, value);
            }
        }

        private int _groupId;
        public int GroupId
        {
            get { return this._groupId; }
            set { this.SetProperty(ref this._groupId, value); }
        }


        private string _description = string.Empty;
        public string Description
        {
            get { return this._description; }
            set { this.SetProperty(ref this._description, value); }
        }

        public override string ToString()
        {
            return this.Title;
        }

        public abstract double Average
        {
            get;
        }
    }

    /// <summary>
    /// Modèle de données d'élément générique.
    /// </summary>
    public class GradeDataItem : GradeDataCommon
    {
        public GradeDataItem(String title, Double ponderation, GradeDataGroup group, String description, Double grade)
            : base(title, ponderation, group, description)
        {
            this._grade = grade;
        }

        /// <summary>
        ///  Constructeur par défaut nécessaire pour SqLite
        /// </summary>
        public GradeDataItem()
            : base()
        {
        }

        private Double _grade = 0.0;
        public Double Grade
        {
            get { return this._grade; }
            set { this.SetProperty(ref this._grade, value); }
        }

        public override double Average
        {
            get { return Math.Round(Ponderation * Grade, 2); }
        }

        public override string ToString()
        {
            return base.ToString() + string.Format(" [ Id = {0} Title = {1} Grade = {2} Pond. = {3}  Desc = {4} GroupId = {5} ]", Id, Title, Grade, Ponderation, Description, GroupId);
        }

    }

    /// <summary>
    /// Modèle de données de groupe générique.
    /// </summary>
    public class GradeDataGroup : GradeDataCommon
    {
        public GradeDataGroup(String title, Double ponderation, GradeDataGroup group, String description)
            : base(title, ponderation, group, description)
        {
            Items.CollectionChanged += ItemsCollectionChanged;
        }

        /// <summary>
        ///  Constructeur spécial pour l'élément root ( pas d'appel à base )
        /// </summary>
        public GradeDataGroup(String title, Double ponderation, int id, int groupId, String description)
        {
            this.Title = title;
            this.Ponderation = ponderation;
            this.Id = id;
            this.GroupId = groupId;
            this.Description = description;
            Items.CollectionChanged += ItemsCollectionChanged;
        }

        /// <summary>
        ///  Constructeur par défaut nécessaire pour SqLite
        /// </summary>
        public GradeDataGroup()
            : base()
        {
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
                        TopItems.Insert(e.NewStartingIndex, Items[e.NewStartingIndex]);
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
        [Ignore]
        public ObservableCollection<GradeDataCommon> Items
        {
            get { return this._items; }
        }

        private ObservableCollection<GradeDataCommon> _topItem = new ObservableCollection<GradeDataCommon>();
        [Ignore]
        public ObservableCollection<GradeDataCommon> TopItems
        {
            get { return this._topItem; }
        }

        public override double Average
        {
            get
            {
                double sumPonderations = 0.0;
                double sumGrades = 0.0;
                foreach (var item in _items)
                {
                    sumPonderations += item.Ponderation;
                    sumGrades += item.Ponderation * item.Average;
                }
                return Math.Round(sumGrades / sumPonderations, 2);
            }
        }

        public override string ToString()
        {
            return base.ToString() + string.Format(" [ Id = {0} Title = {1} Pond. = {2}  Desc = {3} GroupId = {4} ]", Id, Title, Ponderation, Description, GroupId);
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
        private static string DATA_BASE_NAME = "gradeBookDataBase";
        private static int ROOT_ID = -1;
        private static int ROOT_GROUP_ID = -404; // Ne devrait jamais être trouvé
        private static Boolean ARE_DATA_LOADED = false;

        private async void createDatabase()
        {
            try
            {
                SQLiteAsyncConnection conn = new SQLiteAsyncConnection(DATA_BASE_NAME);
                Debug.WriteLine("Database creation : Success");
                await conn.CreateTableAsync<GradeDataItem>();
                Debug.WriteLine("Table Item creation : Success");
                await conn.CreateTableAsync<GradeDataGroup>();
                Debug.WriteLine("Table Group creation : Success");
            }
            catch (Exception e)
            {
                Debug.WriteLine("Database creation : Failed");
                Debug.WriteLine("Message : " + e.Message);
            }
        }

        private async void dropGradeDataTables()
        {
            try
            {
                SQLiteAsyncConnection conn = new SQLiteAsyncConnection(DATA_BASE_NAME);
                await conn.DropTableAsync<GradeDataItem>();
                await conn.DropTableAsync<GradeDataGroup>();
                Debug.WriteLine("GradeDatabase deletion : Success");
            }
            catch (Exception e)
            {
                Debug.WriteLine("GradeDatabase deletion : Failed");
                Debug.WriteLine("Message : " + e.Message);
            }
        }

        private async void createGradeDataItem()
        {
            try
            {
                SQLiteAsyncConnection conn = new SQLiteAsyncConnection(DATA_BASE_NAME);
                GradeDataGroup firstOne = new GradeDataGroup("First group", 1.0, this._rootGroup, "first group");
                //firstOne.GroupId = ROOT_ID;
                Debug.WriteLine(firstOne.ToString());
                Debug.WriteLine("GradeDataItem object creation : Success");

                await conn.InsertAsync(firstOne);
                Debug.WriteLine("GradeDataItem insert : Success");
            }
            catch (Exception e)
            {
                Debug.WriteLine("GradeDataItem insert : Failed");
                Debug.WriteLine("Message : " + e.Message);
            }
        }

        private async void loadGradeData(GradeDataGroup parent)
        {
            try
            {
                SQLiteAsyncConnection conn = new SQLiteAsyncConnection(DATA_BASE_NAME);
                var queryItems = conn.Table<GradeDataItem>().Where(x => x.GroupId == parent.Id);
                Debug.WriteLine("GradeDataItem query : Success");
                var resultItems = await queryItems.ToListAsync();
                Debug.WriteLine("Number of element in query : " + resultItems.Count);

                foreach (var item in resultItems)
                {
                    Debug.WriteLine(item.ToString());
                    parent.Items.Add(item);
                    item.Group = parent;
                }

                var queryGroups = conn.Table<GradeDataGroup>().Where(x => x.GroupId == parent.Id);
                Debug.WriteLine("GradeDataGroup query : Success");
                var resultGroups = await queryGroups.ToListAsync();
                Debug.WriteLine("Number of element in query : " + resultGroups.Count);

                foreach (var group in resultGroups)
                {
                    Debug.WriteLine(group.ToString());
                    parent.Items.Add(group);
                    group.Group = parent;
                    loadGradeData(group);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("GradeDataItem query : Failed");
                Debug.WriteLine("Message : " + e.Message);
            }
        }

        private static GradeDataSource _GradeDataSource = new GradeDataSource();

        private GradeDataGroup _rootGroup = new GradeDataGroup("Grade Book", 1.0, ROOT_ID, ROOT_GROUP_ID, "You can orderd your notes by group");
        public GradeDataGroup RootGroup
        {
            get { return this._rootGroup; }
        }

        public GradeDataSource()
        {

            Debug.WriteLine(this._rootGroup.ToString());
            //createDatabase();
            //dropGradeDataTables();
            //createGradeDataItem();
            loadGradeData(this._rootGroup);
            ARE_DATA_LOADED = true;




            /*
            Debug.WriteLine("My data" + Windows.Storage.ApplicationData.Current.LocalFolder.Path);

            var group1 = new GradeDataGroup(
                    "HE-ARC-1 G",
                    1.0, null, "The funniest science year");
                    
            group1.Items.Add(new GradeDataItem(
                    "Projet 1 I",
                    1.0,
                    group1,
                    "Look at the start",
                    5.0));

            group1.Items.Add(new GradeDataItem(
                    "Sciences-1 I",
                    1.0,
                    group1,
                    "Enjoy vectors",
                    4.0));

            var group11 = new GradeDataGroup(
                    "Programmation G",
                    1.0,
                    group1,
                    "Hello world");
            group11.Items.Add(new GradeDataItem(
                    "Assembleur I",
                    1.0,
                    group11,
                    "LDAA ADDA STAA",
                    5.5));
            group1.Items.Add(group11);

            this._rootGroup.Items.Add(group1);

            var group2 = new GradeDataGroup(
                    "HE-ARC-2 G",
                    1.0, 
                    null,
                    "Try project from scratch");
            group2.Items.Add(new GradeDataItem(
                    "Projet-2 I",
                    1.0,
                    group2,
                    "Youhoooo, shiny!",
                    4.8));
            this._rootGroup.Items.Add(group2);

            var imageNum = new GradeDataGroup(
                    "Imagerie numérique",
                    1.0,
                    null,
                    @"Les objectifs d’apprentissage de ce module sont classés selon les trois degrés croissants de difficulté:
(M) Mémorisation, (A) Application et compréhension, (R) Résolution de problèmes (analyse, synthèse, évaluation).
A l’issue du module, l'étudiant doit être capable de :

- Reproduire les exercices faits en classe. (M)
- Appliquer ce qui a été appris en classe dans des situations nouvelles. (A)
- Réaliser, tester et programmer des applications selon un cahier des charges. (R)

Evaluation des apprentissages

-	Evaluations des différentes Unités d’Enseignement (UE)
-	Un examen oral de 30 min sur « Infographie» et «Traitement d’image » à la fin du semestre de printemps 

");
            var trimg = new GradeDataGroup(
                    "Traitement d'image",
                    1.0,
                    imageNum,
                    @"Identifiant	3252.2


Méthode d’enseignement

    Cours et travaux pratiques en laboratoire
    
    
Objectifs spécifiques

    A l'issue du module, l'étudiant doit être capable de :
-	Décrire les caractéristiques des images et des principaux algorithmes de traitements y relatifs. (M) 
-	Concevoir une application de traitement d’image. (R)
-	Implémenter et/ou utiliser les principaux algorithmes de traitement de l'image. (R)


Modalités d’évaluation

-	Deux contrôles principaux (CP) écrits, annoncés et obligatoires
-	Un projet en groupe ou individuel à rendre en fin d'année
-	Un examen oral à la fin du semestre de printemps


Description du contenu (mots-clés)

-	Caractéristiques des images
-	Chaîne d'acquisition d'images
-	Bruit et Filtrage
-	Convolution
-	Comparaison d’images
-	Morphologique mathématique
-	Détection de contours
-	Segmentation 
-	Caractérisation
-	Transformée de Fourrier


Supports de cours	Au choix de l’enseignant

Outils utilisés	Si des outils (informatiques, par exemple) sont utilisés, ils sont à préciser par le responsable de l’unité d’enseignement au début du cours


Particularité d’organisation	

Environ 2h30 de travail personnel par semaine
");
            imageNum.Items.Add(trimg);
            var infogra = new GradeDataGroup(
                    "Infographie",
                    1.0,
                    imageNum,
                    "Un cours bidon");
            imageNum.Items.Add(infogra);

            infogra.Items.Add(new GradeDataItem(
                    "Quick Quiz 1",
                    1.0,
                    infogra,
                    "7 minutes...",
                    6.0));
            infogra.Items.Add(new GradeDataItem(
                    "Quick Quiz 2",
                    1.0,
                    infogra,
                    "7 minutes...",
                    6.0));
            infogra.Items.Add(new GradeDataItem(
                    "Quick Quiz 3",
                    1.0,
                    infogra,
                    "7 minutes...",
                    6.0));
            infogra.Items.Add(new GradeDataItem(
                    "Quick Quiz 4",
                    1.0,
                    infogra,
                    "7 minutes...",
                    6.0));
            infogra.Items.Add(new GradeDataItem(
                    "Quick Quiz 5",
                    1.0,
                    infogra,
                    "7 minutes...",
                    6.0));
            infogra.Items.Add(new GradeDataItem(
                    "Quick Quiz 6",
                    1.0,
                    infogra,
                    "7 minutes...",
                    6.0));
            infogra.Items.Add(new GradeDataItem(
                    "Quick Quiz 7",
                    1.0,
                    infogra,
                    "7 minutes...",
                    6.0));
            infogra.Items.Add(new GradeDataItem(
                    "Quick Quiz 8",
                    1.0,
                    infogra,
                    "7 minutes...",
                    6.0));
            infogra.Items.Add(new GradeDataItem(
                    "Quick Quiz 9",
                    1.0,
                    infogra,
                    "7 minutes...",
                    6.0));
            infogra.Items.Add(new GradeDataItem(
                    "Quick Quiz 10",
                    1.0,
                    infogra,
                    "7 minutes...",
                    6.0));
            infogra.Items.Add(new GradeDataItem(
                    "Quick Quiz 11",
                    1.0,
                    infogra,
                    "7 minutes...",
                    6.0));
            infogra.Items.Add(new GradeDataItem(
                    "Quick Quiz 12",
                    1.0,
                    infogra,
                    "7 minutes...",
                    6.0));
            infogra.Items.Add(new GradeDataItem(
                    "Quick Quiz 13",
                    1.0,
                    infogra,
                    "7 minutes...",
                    6.0));
            infogra.Items.Add(new GradeDataItem(
                    "Quick Quiz 14",
                    1.0,
                    infogra,
                    "7 minutes...",
                    6.0));
            trimg.Items.Add(new GradeDataItem(
                    "Test 1",
                    1.0,
                    trimg,
                    "Hae duae provinciae bello quondam piratico catervis mixtae praedonum a Servilio pro consule missae sub iugum factae sunt vectigales. et hae quidem regiones velut in prominenti terrarum lingua positae ob orbe eoo monte Amano disparantur.",
                    6.0));

            trimg.Items.Add(new GradeDataItem(
                    "Projet",
                    1.0,
                    trimg,
                    "Hae duae provinciae bello quondam piratico catervis mixtae praedonum a Servilio pro consule missae sub iugum factae sunt vectigales. et hae quidem regiones velut in prominenti terrarum lingua positae ob orbe eoo monte Amano disparantur.",
                    6.0));

            this._rootGroup.Items.Add(imageNum);
         */
        }
    }
}
