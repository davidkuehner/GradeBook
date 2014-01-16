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
        public static string DATA_BASE_NAME = "gradeBookDataBase";

        private static double GRADE_MIN = 4.0;

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

        [Ignore]
        public ImageSource Image
        {
            get
            {
                if (this.Ponderation == 0.0)
                {
                    return new BitmapImage(new Uri(GradeDataCommon._baseUri, "Assets/gray.png"));
                }
                else if (this.Average < GRADE_MIN)
                {
                    return new BitmapImage(new Uri(GradeDataCommon._baseUri, "Assets/red.png"));
                }
                else
                {
                    return new BitmapImage(new Uri(GradeDataCommon._baseUri, "Assets/green.png"));
                }
            }
        }

        public abstract void databaseDelete();

        public abstract void databaseUpdate();
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
            get { return Math.Round(Grade, 2); }
        }

        public override string ToString()
        {
            return base.ToString() + string.Format(" [ Id = {0} Title = {1} Grade = {2} Pond. = {3}  Desc = {4} GroupId = {5} ]", Id, Title, Grade, Ponderation, Description, GroupId);
        }

        public async override void databaseDelete()
        {
            SQLiteAsyncConnection conn = new SQLiteAsyncConnection(DATA_BASE_NAME);
            await conn.DeleteAsync(this);
            Group.Items.Remove(this);
        }

        public async override void databaseUpdate()
        {
            SQLiteAsyncConnection conn = new SQLiteAsyncConnection(GradeDataCommon.DATA_BASE_NAME);
            await conn.UpdateAsync(this);
            
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

                if (sumPonderations == 0.0)
                {
                    return 0.0;
                }
                else
                {
                    return Math.Round(sumGrades / sumPonderations, 2);
                }
            }
        }

        public override string ToString()
        {
            return base.ToString() + string.Format(" [ Id = {0} Title = {1} Pond. = {2}  Desc = {3} GroupId = {4} ]", Id, Title, Ponderation, Description, GroupId);
        }

        public async override void databaseDelete()
        {
            SQLiteAsyncConnection conn = new SQLiteAsyncConnection(DATA_BASE_NAME);
            await conn.DeleteAsync(this);
            foreach (var child in this.Items)
            {
                child.databaseDelete();
            }
            Group.Items.Remove(this);
        }

        public async override void databaseUpdate()
        {
            SQLiteAsyncConnection conn = new SQLiteAsyncConnection(GradeDataCommon.DATA_BASE_NAME);
            await conn.UpdateAsync(this);
        }

        public async void databaseAppendNewGroup()
        {
            SQLiteAsyncConnection conn = new SQLiteAsyncConnection(GradeDataCommon.DATA_BASE_NAME);

            GradeDataGroup child = new GradeDataGroup("New group", 0, this, "Group description");
            this.Items.Add(child);
            await conn.InsertAsync(child);
            await conn.UpdateAsync(this);
        }

        public async void databaseAppendNewItem()
        {
            SQLiteAsyncConnection conn = new SQLiteAsyncConnection(GradeDataCommon.DATA_BASE_NAME);

            GradeDataItem child = new GradeDataItem("New item", 0, this, "Group description", 4.0);
            this.Items.Add(child);
            await conn.InsertAsync(child);
            await conn.UpdateAsync(this);
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
        private static int ROOT_ID = -1;
        private static int ROOT_GROUP_ID = -404; // Ne devrait jamais être trouvé
        private static Boolean CLEAR_DONE_ONCE = false;
        private static Boolean STUB_DONE_ONCE = false;

        private async void createDatabase()
        {
            try
            {
                SQLiteAsyncConnection conn = new SQLiteAsyncConnection(GradeDataCommon.DATA_BASE_NAME);
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
                SQLiteAsyncConnection conn = new SQLiteAsyncConnection(GradeDataCommon.DATA_BASE_NAME);
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

        private async void createGradeDataStub()
        {
            if (STUB_DONE_ONCE == false)
            {
                STUB_DONE_ONCE = true;
            try
            {
                SQLiteAsyncConnection conn = new SQLiteAsyncConnection(GradeDataCommon.DATA_BASE_NAME);

                Debug.WriteLine("\nGradeDataStub creation : Beginning\n");
                
                // Ecole d'Art Appliqué /////////////////////////////////////////////////
                GradeDataGroup EAA = new GradeDataGroup("Ecole d'Arts Appliqués", 0, this._rootGroup, "Le concepteur multimédia conçoit des supports imprimés (print) et électroniques (screen). Il mène des réflexions, rédige et/ou suit des cahiers des charges, possède un sens analytique, esthétique et de communication afin de servir l’information et les messages pour des publics cibles spécifiques.\nIl développe des projets de communication éditoriaux, visuels et interactifs. Il maîtrise aussi bien les techniques et les outils de l’image que du son numérique. Il allie le discours et la forme au service de visuels fixes ou animés, interactifs ou inactifs, constitués de mise en page, de typographie, d’images fixes ou dynamiques et de sons.\nLe concepteur multimédia mène à bien l’intégralité d’une production multimédia. Il conçoit et réalise des produits en définissant le contexte culturel, économique et technologique du projet. Il a les facultés de travailler avec des partenaires spécialisés, voire de mener une équipe.");
                this._rootGroup.Items.Add(EAA);
                await conn.InsertAsync(EAA);
                Debug.WriteLine("GradeDataGroup insert " + EAA.Title + " : Success");
                Debug.WriteLine(EAA.ToString());

                // 1ère année
                GradeDataItem EAA1 = new GradeDataItem("1ère annnée", 1, EAA, "Année de tronc commun, connaissances artistiques pures.", 5.1 );
                EAA.Items.Add(EAA1);
                await conn.InsertAsync(EAA1);
                Debug.WriteLine("GradeDataItem insert " + EAA1.Title + ": Success");
                Debug.WriteLine(EAA1.ToString());

                // 2ème année
                GradeDataItem EAA2 = new GradeDataItem("2ème annnée", 1, EAA, "Apprentissage des bases dans de nombreux domaines : graphisme, traitement d'image, typographie, multimédia, vidéo, projets.", 4.8);
                EAA.Items.Add(EAA2);
                await conn.InsertAsync(EAA2);
                Debug.WriteLine("GradeDataItem insert " + EAA2.Title + " : Success");
                Debug.WriteLine(EAA2.ToString());

                // 3ème année
                GradeDataItem EAA3 = new GradeDataItem("3ème annnée", 1, EAA, "Approfondissement des connaissances : multimédia, photographie, communication, marketing, vidéo typographie", 4.8);
                EAA.Items.Add(EAA3);
                await conn.InsertAsync(EAA3);
                Debug.WriteLine("GradeDataItem insert " + EAA3.Title + " : Success");
                Debug.WriteLine(EAA3.ToString());

                // 4ème année
                GradeDataItem EAA4 = new GradeDataItem("4ème annnée", 1, EAA, "Application et spécialisation des connaissances : multimédia, typographie, photographie, communication, marketing, projet", 4.8);
                EAA.Items.Add(EAA4);
                await conn.InsertAsync(EAA4);
                Debug.WriteLine("GradeDataItem insert " + EAA4.Title + " : Success");
                Debug.WriteLine(EAA4.ToString());

                // Maturité professionnelle artistique /////////////////////////////////////////////////
                GradeDataGroup MPA = new GradeDataGroup("Maturité pro. artistique", 0, this._rootGroup, "La MPArt offre une formation culturelle de base élargie et une formation professionnelle qualifiée dans le but spécifique d’assurer la préparation adéquate à la fréquentation d’une Haute école spécialisée (HES) dans le domaine des arts appliqués.");
                this._rootGroup.Items.Add(MPA);
                await conn.InsertAsync(MPA);
                Debug.WriteLine("GradeDataGroup insert " + MPA.Title + " : Success");
                Debug.WriteLine(MPA.ToString());

                // Français
                GradeDataItem FRA = new GradeDataItem("Français", 1, MPA, "Français classique, litérature, dissertation...", 5.3);
                MPA.Items.Add(FRA);
                await conn.InsertAsync(FRA);
                Debug.WriteLine("GradeDataItem insert " + FRA.Title + ": Success");
                Debug.WriteLine(FRA.ToString());

                // Allemand
                GradeDataItem ALL = new GradeDataItem("Allemand", 1, MPA, "Allemand classique, grammaire, vocabulaire, dissertation...", 4.8);
                MPA.Items.Add(ALL);
                await conn.InsertAsync(ALL);
                Debug.WriteLine("GradeDataItem insert " + ALL.Title + ": Success");
                Debug.WriteLine(ALL.ToString());

                // Anglais
                GradeDataItem ANG = new GradeDataItem("Anglais", 1, MPA, "Anglais classique, grammaire, vocabulaire, dissertation...", 5.5);
                MPA.Items.Add(ANG);
                await conn.InsertAsync(ANG);
                Debug.WriteLine("GradeDataItem insert " + ANG.Title + ": Success");
                Debug.WriteLine(ANG.ToString());

                // Histoire
                GradeDataItem HIS = new GradeDataItem("Histoire et institution politique", 1, MPA, "Connaitre son passé, comprendre le présent, prévoir l'avenir.", 6.0);
                MPA.Items.Add(HIS);
                await conn.InsertAsync(HIS);
                Debug.WriteLine("GradeDataItem insert " + HIS.Title + ": Success");
                Debug.WriteLine(HIS.ToString());

                // Economie
                GradeDataItem ECO = new GradeDataItem("Economie", 1, MPA, "Economie d'entreprise, droit et économie politique", 5.5);
                MPA.Items.Add(ECO);
                await conn.InsertAsync(ECO);
                Debug.WriteLine("GradeDataItem insert " + ECO.Title + ": Success");
                Debug.WriteLine(ECO.ToString());

                // Mathématiques
                GradeDataItem MAT = new GradeDataItem("Mathématiques", 1, MPA, "1+1 = 2   \0/", 5.8);
                MPA.Items.Add(MAT);
                await conn.InsertAsync(MAT);
                Debug.WriteLine("GradeDataItem insert " + MAT.Title + ": Success");
                Debug.WriteLine(MAT.ToString());

                // Culture et art
                GradeDataItem ART = new GradeDataItem("Culture et art", 1, MPA, "Histoire de l'art, art plastique, application et théorie...", 5.3);
                MPA.Items.Add(ART);
                await conn.InsertAsync(ART);
                Debug.WriteLine("GradeDataItem insert " + ART.Title + ": Success");
                Debug.WriteLine(ART.ToString());

                // Travail personnel
                GradeDataItem TPE = new GradeDataItem("Travail personnel", 1, MPA, "Conception et réalisation d'un journale artistique régional", 6.0);
                MPA.Items.Add(TPE);
                await conn.InsertAsync(TPE);
                Debug.WriteLine("GradeDataItem insert " + TPE.Title + ": Success");
                Debug.WriteLine(TPE.ToString());

                // HE-Arc /////////////////////////////////////////////////
                GradeDataGroup HEA = new GradeDataGroup("Haute école ARC - ingénierie", 0, this._rootGroup, "L’ingénieur-e ayant obtenu le Bachelor en informatique pourra être engagé par des entreprises les plus diverses où il occupera des postes tels qu’ingénieur de développement, ingénieur technico-commercial, consultant, etc.\nSes compétences en gestion de projets lui permettront de faire évoluer sa carrière vers des responsabilités de projets, la gestion d’équipes ou la création d’entreprise.");
                this._rootGroup.Items.Add(HEA);
                await conn.InsertAsync(HEA);
                Debug.WriteLine("GradeDataGroup insert " + HEA.Title + " : Success");
                Debug.WriteLine(HEA.ToString());

                // 1ère année
                GradeDataGroup HEA1 = new GradeDataGroup("1ère année", 1, HEA, "Tronc commun ou comment faire des sciences en masse.");
                HEA.Items.Add(HEA1);
                await conn.InsertAsync(HEA1);
                Debug.WriteLine("GradeDataItem insert " + HEA1.Title + ": Success");
                Debug.WriteLine(HEA1.ToString());

                    // Communication
                    GradeDataItem COM = new GradeDataItem("Langues", 1, HEA1, "", 5.9);
                    HEA1.Items.Add(COM);
                    await conn.InsertAsync(COM);
                    Debug.WriteLine("GradeDataItem insert " + COM.Title + ": Success");
                    Debug.WriteLine(COM.ToString());

                    // Sciences 1A
                    GradeDataItem SCA = new GradeDataItem("Sciences 1A", 1, HEA1, "", 4.2);
                    HEA1.Items.Add(SCA);
                    await conn.InsertAsync(SCA);
                    Debug.WriteLine("GradeDataItem insert " + SCA.Title + ": Success");
                    Debug.WriteLine(SCA.ToString());

                    // Sciences 1B
                    GradeDataItem SCB = new GradeDataItem("Sciences 1B", 1, HEA1, "", 4.0);
                    HEA1.Items.Add(SCB);
                    await conn.InsertAsync(SCB);
                    Debug.WriteLine("GradeDataItem insert " + SCB.Title + ": Success");
                    Debug.WriteLine(SCB.ToString());

                    // Programmation 1
                    GradeDataItem PRG = new GradeDataItem("Programmation 1", 1, HEA1, "Hello world !", 5.2);
                    HEA1.Items.Add(PRG);
                    await conn.InsertAsync(PRG);
                    Debug.WriteLine("GradeDataItem insert " + PRG.Title + ": Success");
                    Debug.WriteLine(PRG.ToString());

                    // OS et réseaux
                    GradeDataItem OSR = new GradeDataItem("OS et réseaux", 1, HEA1, "", 5.0);
                    HEA1.Items.Add(OSR);
                    await conn.InsertAsync(OSR);
                    Debug.WriteLine("GradeDataItem insert " + OSR.Title + ": Success");
                    Debug.WriteLine(OSR.ToString());

                    // Sys num et elect
                    GradeDataItem SYS = new GradeDataItem("Systèmes numériques et éléctronique ", 1, HEA1, "OR XOR AND", 4.7);
                    HEA1.Items.Add(SYS);
                    await conn.InsertAsync(SYS);
                    Debug.WriteLine("GradeDataItem insert " + SYS.Title + ": Success");
                    Debug.WriteLine(SYS.ToString());

                    // Projet P1
                    GradeDataItem PR1 = new GradeDataItem("Projet P1", 1, HEA1, "Pointeur d'étoiles", 5.8);
                    HEA1.Items.Add(PR1);
                    await conn.InsertAsync(PR1);
                    Debug.WriteLine("GradeDataItem insert " + PR1.Title + ": Success");
                    Debug.WriteLine(PR1.ToString());

                // 2eme année
                GradeDataGroup HEA2 = new GradeDataGroup("2ème année", 1, HEA, "Do it your self");
                HEA.Items.Add(HEA2);
                await conn.InsertAsync(HEA2);
                Debug.WriteLine("GradeDataItem insert " + HEA2.Title + ": Success");
                Debug.WriteLine(HEA2.ToString());

                    // Sciences 2
                    GradeDataItem SC2 = new GradeDataItem("Sciences 2", 1, HEA2, "Analyse, probabilités et statistiques", 4.7);
                    HEA2.Items.Add(SC2);
                    await conn.InsertAsync(SC2);
                    Debug.WriteLine("GradeDataItem insert " + SC2.Title + ": Success");
                    Debug.WriteLine(SC2.ToString());

                    // Langages et FW
                    GradeDataItem LFW = new GradeDataItem("Langages et frameworks", 1, HEA2, "Analyse, probabilités et statistiques", 5.1);
                    HEA2.Items.Add(LFW);
                    await conn.InsertAsync(LFW);
                    Debug.WriteLine("GradeDataItem insert " + LFW.Title + ": Success");
                    Debug.WriteLine(LFW.ToString());

                    // Techniques de conception
                    GradeDataItem TCO = new GradeDataItem("Techniques de conception", 1, HEA2, "Analyse, probabilités et statistiques", 5.4);
                    HEA2.Items.Add(TCO);
                    await conn.InsertAsync(TCO);
                    Debug.WriteLine("GradeDataItem insert " + TCO.Title + ": Success");
                    Debug.WriteLine(TCO.ToString());

                    // Techniques de modélisation
                    GradeDataItem TCM = new GradeDataItem("Techniques de modélisation", 1, HEA2, "Analyse, probabilités et statistiques", 5.4);
                    HEA2.Items.Add(TCM);
                    await conn.InsertAsync(TCM);
                    Debug.WriteLine("GradeDataItem insert " + TCM.Title + ": Success");
                    Debug.WriteLine(TCM.ToString());

                    // Réseaux et internet
                    GradeDataItem RIT = new GradeDataItem("Réseaux et internet", 1, HEA2, "Analyse, probabilités et statistiques", 5.0);
                    HEA2.Items.Add(RIT);
                    await conn.InsertAsync(RIT);
                    Debug.WriteLine("GradeDataItem insert " + RIT.Title + ": Success");
                    Debug.WriteLine(RIT.ToString());

                    // Projet P2
                    GradeDataItem PR2 = new GradeDataItem("Projet P2", 1, HEA2, "Analyse, probabilités et statistiques", 6.0);
                    HEA2.Items.Add(PR2);
                    await conn.InsertAsync(PR2);
                    Debug.WriteLine("GradeDataItem insert " + PR2.Title + ": Success");
                    Debug.WriteLine(PR2.ToString());

                // 3ème année
                GradeDataGroup HEA3 = new GradeDataGroup("3ème année", 1, HEA, "Projet, projet, projet, projet, projet, projet, projet, ...");
                HEA.Items.Add(HEA3);
                await conn.InsertAsync(HEA3);
                Debug.WriteLine("GradeDataItem insert " + HEA3.Title + ": Success");
                Debug.WriteLine(HEA3.ToString());

                    // Gestion 
                    GradeDataGroup GST = new GradeDataGroup("Gestion", 1, HEA3, "Fonctionnement des entreprises, communication interpersonnelle, méthodologie.");
                    HEA3.Items.Add(GST);
                    await conn.InsertAsync(GST);
                    Debug.WriteLine("GradeDataItem insert " + GST.Title + ": Success");
                    Debug.WriteLine(GST.ToString());

                        // Qualité logiciel 
                        GradeDataItem QLO = new GradeDataItem("Qualité logiciel", 1, GST, "Citer les critères de qualité du logiciel. Reconnaître le type et les limites d’une licence. Pour un projet logiciel donné, identifier les risques généraux. Proposer une méthodologie de test dans un cas pratique donné. Proposer une méthodologie de développement adaptée à un cas pratique donné.", 6.0);
                        GST.Items.Add(QLO);
                        await conn.InsertAsync(QLO);
                        Debug.WriteLine("GradeDataItem insert " + QLO.Title + ": Success");
                        Debug.WriteLine(QLO.ToString());

                        // Gestion et économie d'entreprise
                        GradeDataItem GEE = new GradeDataItem("Gestion et économie d'entreprise", 0.5, GST, "Juridique et économique", 3.8);
                        GST.Items.Add(GEE);
                        await conn.InsertAsync(GEE);
                        Debug.WriteLine("GradeDataItem insert " + GEE.Title + ": Success");
                        Debug.WriteLine(GEE.ToString());

                    // Imagerie numérique 
                    GradeDataItem IMG = new GradeDataItem("Imagerie numérique", 1, HEA3, "Traitement d'image et infographie", 5.62);
                    HEA3.Items.Add(IMG);
                    await conn.InsertAsync(IMG);
                    Debug.WriteLine("GradeDataItem insert " + IMG.Title + ": Success");
                    Debug.WriteLine(IMG.ToString());

                    // Développement système
                    GradeDataItem DVS = new GradeDataItem("Développement système", 0, HEA3, "Bases de l'informatique", 0.0);
                    HEA3.Items.Add(DVS);
                    await conn.InsertAsync(DVS);
                    Debug.WriteLine("GradeDataItem insert " + DVS.Title + ": Success");
                    Debug.WriteLine(DVS.ToString());

                    // Développement web et mobile
                    GradeDataItem DWM = new GradeDataItem("Développement web et mobile", 0, HEA3, "Utilisation de framework.", 0.0);
                    HEA3.Items.Add(DWM);
                    await conn.InsertAsync(DWM);
                    Debug.WriteLine("GradeDataItem insert " + DWM.Title + ": Success");
                    Debug.WriteLine(DWM.ToString());

                    // IA et cours à choix
                    GradeDataItem IAE = new GradeDataItem("IA et cours à choix", 0, HEA3, "Comprendre \"cours à choix\" = JEE", 0.0);
                    HEA3.Items.Add(IAE);
                    await conn.InsertAsync(IAE);
                    Debug.WriteLine("GradeDataItem insert " + IAE.Title + ": Success");
                    Debug.WriteLine(IAE.ToString());

                Debug.WriteLine("\nGradeDataStub creation : Ending\n");
            }
            catch (Exception e)
            {
                Debug.WriteLine("GradeDataItem insert : Failed");
                Debug.WriteLine("Message : " + e.Message);
            }
            
            }
        }

        private async void loadGradeData(GradeDataGroup parent)
        {
            try
            {
                SQLiteAsyncConnection conn = new SQLiteAsyncConnection(GradeDataCommon.DATA_BASE_NAME);
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

        private void clearAndCreateDatabase()
        {
            if (CLEAR_DONE_ONCE == false)
            {
                CLEAR_DONE_ONCE = true;
                dropGradeDataTables();
                createDatabase();              
            }
        }

        public GradeDataSource()
        {
            // DevTools
            // DevTools : Supression et génération de la base de données
            //clearAndCreateDatabase(); // Ne pas executer en meme temps ces deux méthodes.
            //createGradeDataStub();

            // Check si la base de données est disponible, si non la créé.
            createDatabase();
            loadGradeData(this._rootGroup);
        }
    }
}
