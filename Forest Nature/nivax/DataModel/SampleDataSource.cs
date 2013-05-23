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

// The data model defined by this file serves as a representative example of a strongly-typed
// model that supports notification when members are added, removed, or modified.  The property
// names chosen coincide with data bindings in the standard item templates.
//
// Applications may use this model as a starting point and build on it, or discard it entirely and
// replace it with something appropriate to their needs.

namespace nivax.Data
{
    /// <summary>
    /// Base class for <see cref="SampleDataItem"/> and <see cref="SampleDataGroup"/> that
    /// defines properties common to both.
    /// </summary>
    [Windows.Foundation.Metadata.WebHostHidden]
    public abstract class SampleDataCommon : nivax.Common.BindableBase
    {
        private static Uri _baseUri = new Uri("ms-appx:///");

        public SampleDataCommon(String uniqueId, String title, String subtitle, String imagePath, String description)
        {
            this._uniqueId = uniqueId;
            this._title = title;
            this._subtitle = subtitle;
            this._description = description;
            this._imagePath = imagePath;
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

        private string _subtitle = string.Empty;
        public string Subtitle
        {
            get { return this._subtitle; }
            set { this.SetProperty(ref this._subtitle, value); }
        }

        private string _description = string.Empty;
        public string Description
        {
            get { return this._description; }
            set { this.SetProperty(ref this._description, value); }
        }

        private ImageSource _image = null;
        private String _imagePath = null;
        public ImageSource Image
        {
            get
            {
                if (this._image == null && this._imagePath != null)
                {
                    this._image = new BitmapImage(new Uri(SampleDataCommon._baseUri, this._imagePath));
                }
                return this._image;
            }

            set
            {
                this._imagePath = null;
                this.SetProperty(ref this._image, value);
            }
        }

        public void SetImage(String path)
        {
            this._image = null;
            this._imagePath = path;
            this.OnPropertyChanged("Image");
        }

        public override string ToString()
        {
            return this.Title;
        }
    }

    /// <summary>
    /// Generic item data model.
    /// </summary>
    public class SampleDataItem : SampleDataCommon
    {
        public SampleDataItem(String uniqueId, String title, String subtitle, String imagePath, String description, String content, SampleDataGroup group)
            : base(uniqueId, title, subtitle, imagePath, description)
        {
            this._content = content;
            this._group = group;
        }

        private string _content = string.Empty;
        public string Content
        {
            get { return this._content; }
            set { this.SetProperty(ref this._content, value); }
        }

        private SampleDataGroup _group;
        public SampleDataGroup Group
        {
            get { return this._group; }
            set { this.SetProperty(ref this._group, value); }
        }
    }

    /// <summary>
    /// Generic group data model.
    /// </summary>
    public class SampleDataGroup : SampleDataCommon
    {
        public SampleDataGroup(String uniqueId, String title, String subtitle, String imagePath, String description)
            : base(uniqueId, title, subtitle, imagePath, description)
        {
            Items.CollectionChanged += ItemsCollectionChanged;
        }

        private void ItemsCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            // Provides a subset of the full items collection to bind to from a GroupedItemsPage
            // for two reasons: GridView will not virtualize large items collections, and it
            // improves the user experience when browsing through groups with large numbers of
            // items.
            //
            // A maximum of 12 items are displayed because it results in filled grid columns
            // whether there are 1, 2, 3, 4, or 6 rows displayed

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

        private ObservableCollection<SampleDataItem> _items = new ObservableCollection<SampleDataItem>();
        public ObservableCollection<SampleDataItem> Items
        {
            get { return this._items; }
        }

        private ObservableCollection<SampleDataItem> _topItem = new ObservableCollection<SampleDataItem>();
        public ObservableCollection<SampleDataItem> TopItems
        {
            get {return this._topItem; }
        }
    }

    /// <summary>
    /// Creates a collection of groups and items with hard-coded content.
    /// 
    /// SampleDataSource initializes with placeholder data rather than live production
    /// data so that sample data is provided at both design-time and run-time.
    /// </summary>
    public sealed class SampleDataSource
    {
        private static SampleDataSource _sampleDataSource = new SampleDataSource();

        private ObservableCollection<SampleDataGroup> _allGroups = new ObservableCollection<SampleDataGroup>();
        public ObservableCollection<SampleDataGroup> AllGroups
        {
            get { return this._allGroups; }
        }

        public static IEnumerable<SampleDataGroup> GetGroups(string uniqueId)
        {
            if (!uniqueId.Equals("AllGroups")) throw new ArgumentException("Only 'AllGroups' is supported as a collection of groups");
            
            return _sampleDataSource.AllGroups;
        }

        public static SampleDataGroup GetGroup(string uniqueId)
        {
            // Simple linear search is acceptable for small data sets
            var matches = _sampleDataSource.AllGroups.Where((group) => group.UniqueId.Equals(uniqueId));
            if (matches.Count() == 1) return matches.First();
            return null;
        }

        public static SampleDataItem GetItem(string uniqueId)
        {
            // Simple linear search is acceptable for small data sets
            var matches = _sampleDataSource.AllGroups.SelectMany(group => group.Items).Where((item) => item.UniqueId.Equals(uniqueId));
            if (matches.Count() == 1) return matches.First();
            return null;
        }

        public SampleDataSource()
        {
            String ITEM_CONTENT = String.Format("Item Content: {0}",
                        "nivax");

            var group1 = new SampleDataGroup("Group-1",
                    "Introduction",
                    "",
                    "Assets/10.png",
                    "");
            group1.Items.Add(new SampleDataItem("Group-1-Item-1",
                    "Forest",
                    "",
                    "Assets/11.png",
                    "A forest, also referred to as a wood or the woods, is an area with a high density of trees.",
                    "A forest, also referred to as a wood or the woods, is an area with a high density of trees. As with cities, depending on various cultural definitions, what is considered a forest may vary significantly in size and have different classifications according to how and of what the forest is composed. A forest is usually an area filled with trees but any tall densely packed area of vegetation may be considered a forest, even underwater vegetation such as kelp forests, or non-vegetation such as fungi, and bacteria. Tree forests cover approximately 9.4 percent of the Earth's surface (or 30 percent of total land area), though they once covered much more (about 50 percent of total land area). They function as habitats for organisms, hydrologic flow modulators, and soil conservers, constituting one of the most important aspects of the biosphere. A typical tree forest is composed of the overstory (canopy or upper tree layer) and the understory. The understory is further subdivided into the shrub layer, herb layer, and also the moss layer and soil microbes. In some complex forests, there is also a well-defined lower tree layer. Forests are central to all human life because they provide a diverse range of resources: they store carbon, aid in regulating the planetary climate, purify water and mitigate natural hazards such as floods. Forests also contain roughly 90 percent of the world's terrestrial biodiversity.",
                    group1));
            group1.Items.Add(new SampleDataItem("Group-1-Item-2",
                    "Etymology",
                    "",
                    "Assets/12.png",
                    "The word forest comes from Middle English forest, from Old French forest (also forès) forest, vast expanse covered by trees first introduced in English as the word for wild land set aside for hunting[4] without the necessity in definition for the existence of trees",
                    "The word forest comes from Middle English forest, from Old French forest (also forès) forest, vast expanse covered by trees first introduced in English as the word for wild land set aside for hunting[4] without the necessity in definition for the existence of trees (James 1981;Muir 2000,2008).Possibly a borrowing (probably via Frankish or Old High German) of the Medieval Latin word foresta open wood, foresta was first used by Carolingian scribes in the Capitularies of Charlemagne to refer specifically to the king's royal hunting grounds. The term was not endemic to Romance languages (e.g. native words for forest in the Romance languages evolved out of the Latin word silva forest, wood (English sylvan); cf. Italian, Spanish, Portuguese selva; Romanian silvă; Old French selve); and cognates in Romance languages, such as Italian foresta, Spanish and Portuguese floresta, etc. are all ultimately borrowings of the French word.\n\n The exact origin of Medieval Latin foresta is obscure. Some authorities claim the word derives from the Late Latin phrase forestam silvam, meaning the outer wood others claim the term is a latinisation of the Frankish word *forhist forest, wooded country, assimilated to forestam silvam (a common practice among Frankish scribes). Frankish *forhist is attested by Old High German forst forest, Middle Low German vorst forest, Old English fyrhþ forest, woodland, game preserve, hunting ground (English frith), and Old Norse fýri coniferous forest, all of which derive from Proto-Germanic *furhísa-, *furhíþija- a fir-wood, coniferous forest, from Proto-Indo-European *perkwu- a coniferous or mountain forest, wooded height.",
                    group1));
            group1.Items.Add(new SampleDataItem("Group-1-Item-3",
                    "Distribution",
                    "",
                    "Assets/13.png",
                    "Forests can be found in all regions capable of sustaining tree growth, at altitudes up to the tree line, except where natural fire frequency or other disturbance is too high, or where the environment has been altered by human activity.",
                    "Forests can be found in all regions capable of sustaining tree growth, at altitudes up to the tree line, except where natural fire frequency or other disturbance is too high, or where the environment has been altered by human activity. The latitudes 10° north and south of the Equator are mostly covered in tropical rainforest, and the latitudes between 53°N and 67°N have boreal forest. As a general rule, forests dominated by angiosperms (broadleaf forests) are more species-rich than those dominated by gymnosperms (conifer, montane, or needleleaf forests), although exceptions exist.\n\nForests sometimes contain many tree species only within a small area (as in tropical rain and temperate deciduous forests), or relatively few species over large areas (e.g., taiga and arid montane coniferous forests). Forests are often home to many animal and plant species, and biomass per unit area is high compared to other vegetation communities. Much of this biomass occurs below ground in the root systems and as partially decomposed plant detritus. The woody component of a forest contains lignin, which is relatively slow to decompose compared with other organic materials such as cellulose or carbohydrate.",
                    group1));
            group1.Items.Add(new SampleDataItem("Group-1-Item-4",
                    "Classification",
                    "",
                    "Assets/14.png",
                    "Forests can be classified in different ways and to different degrees of specificity. One such way is in terms of the biome in which they exist, combined with leaf longevity of the dominant species (whether they are evergreen or deciduous).",
                    "Forests can be classified in different ways and to different degrees of specificity. One such way is in terms of the biome in which they exist, combined with leaf longevity of the dominant species (whether they are evergreen or deciduous).\n\nA number of global forest classification systems have been proposed, but none has gained universal acceptance.[9] UNEP-WCMC's forest category classification system is a simplification of other more complex systems (e.g. UNESCO's forest and woodland 'subformations'). This system divides the world's forests into 26 major types, which reflect climatic zones as well as the principal types of trees. These 26 major types can be reclassified into 6 broader categories: temperate needleleaf; temperate broadleaf and mixed; tropical moist; tropical dry; sparse trees and parkland; and forest plantations. Each category is described as a separate section below.",
                    group1));
            group1.Items.Add(new SampleDataItem("Group-1-Item-5",
                    "Araucariaceae",
                    "",
                    "Assets/15.png",
                    "Araucariaceae, commonly referred to as araucarians, is a very ancient family of coniferous trees. It achieved its maximum diversity in the Jurassic and Cretaceous periods, when it was distributed almost worldwide. At the end of the Cretaceous, when dinosaurs became extinct, so too did the Araucariaceae in the northern hemisphere.",
                    "Araucariaceae, commonly referred to as araucarians, is a very ancient family of coniferous trees. It achieved its maximum diversity in the Jurassic and Cretaceous periods, when it was distributed almost worldwide. At the end of the Cretaceous, when dinosaurs became extinct, so too did the Araucariaceae in the northern hemisphere.\n\nMembers of Araucariaceae are typically very tall evergreen trees, reaching heights of 60 m (200 ft) or more. They can also grow very large. A New Zealand kauri tree (Agathis australis) named Tāne Mahuta (The Lord of the Forest) has been measured at 45.2 m (148 ft) tall with a diameter at breast height of 491 cm (16.11 ft). Its total wood volume is calculated to be 516.7 m3 (18,250 cu ft), making it the third largest conifer after Sequoia and Sequoiadendron (both from the family Cupressaceae).\n\nThe trunks are columnar and have relatively large piths with a resinous cortex.[4] The branching is usually horizontal and tiered, arising regularly in whorls of three to seven branches or alternating in widely separated pairs.\n\nThe leaves can be small, needle-like, and curved or they can be large, broadly ovate, and flattened.[6] They are spirally arranged, persistent, and usually have parallel venation.\n\nEach tree can have both male and female gametes (monoecious) or they can be male or female, but not both (dioecious). Like other conifers, they produce cones.",
                    group1));
            this.AllGroups.Add(group1);

            var group2 = new SampleDataGroup("Group-2",
                    "Impact",
                    "",
                    "Assets/20.png",
                    "");
            group2.Items.Add(new SampleDataItem("Group-2-Item-1",
                    "Temperate needleleaf",
                    "",
                    "Assets/21.png",
                    "Temperate needleleaf forests mostly occupy the higher latitude regions of the northern hemisphere, as well as high altitude zones and some warm temperate areas, especially on nutrient-poor or otherwise unfavourable soils.",
                    "Temperate needleleaf forests mostly occupy the higher latitude regions of the northern hemisphere, as well as high altitude zones and some warm temperate areas, especially on nutrient-poor or otherwise unfavourable soils. These forests are composed entirely, or nearly so, of coniferous species (Coniferophyta). In the Northern Hemisphere pines Pinus, spruces Picea, larches Larix, firs Abies, Douglas firs Pseudotsuga and hemlocks Tsuga, make up the canopy, but other taxa are also important. In the Southern Hemisphere, most coniferous trees (members of the Araucariaceae and Podocarpaceae) occur in mixtures with broadleaf species, and are classed as broadleaf and mixed forests.",
                    group2));
            group2.Items.Add(new SampleDataItem("Group-2-Item-2",
                    "Temperate broadleaf and mixed",
                    "",
                    "Assets/22.png",
                    "Temperate broadleaf and mixed forests include a substantial component of trees in the Anthophyta.",
                    "Temperate broadleaf and mixed forests include a substantial component of trees in the Anthophyta. They are generally characteristic of the warmer temperate latitudes, but extend to cool temperate ones, particularly in the southern hemisphere. They include such forest types as the mixed deciduous forests of the United States and their counterparts in China and Japan, the broadleaf evergreen rainforests of Japan, Chile and Tasmania, the sclerophyllous forests of Australia, central Chile, the Mediterranean and California, and the southern beech Nothofagus forests of Chile and New Zealand.",
                    group2));
            group2.Items.Add(new SampleDataItem("Group-2-Item-3",
                    "Tropical moist",
                    "",
                    "Assets/23.png",
                    "There are many different types of tropical moist forests,although most extensive are the lowland evergreen broad leaf rainforests, for example várzea and igapó forests and the terra firma forests of the Amazon Basin.",
                    "There are many different types of tropical moist forests,although most extensive are the lowland evergreen broad leaf rainforests, for example várzea and igapó forests and the terra firma forests of the Amazon Basin; the peat swamp forests, dipterocarp forests of Southeast Asia; and the high forests of the Congo Basin. Forests located on mountains are also included in this category, divided largely into upper and lower montane formations on the basis of the variation of physiognomy corresponding to changes in altitude.",
                    group2));
            this.AllGroups.Add(group2);

            var group3 = new SampleDataGroup("Group-3",
                    "Directions",
                    "",
                    "Assets/30.png",
                    "");
            group3.Items.Add(new SampleDataItem("Group-3-Item-1",
                    "Forest management",
                    "",
                    "Assets/31.png",
                    "Forest management is a branch of forestry concerned with the overall administrative, economic, legal and social aspects and with the essentially scientific and technical aspects, especially silviculture, protection, and forest regulation.",
                    "Forest management is a branch of forestry concerned with the overall administrative, economic, legal and social aspects and with the essentially scientific and technical aspects, especially silviculture, protection, and forest regulation. This includes management for aesthetics, fish, recreation, urban values, water, wilderness, wildlife, wood products, forest genetic resources and other forest resource values.[1] Management can be based on conservation, economics, or a mixture of the two. Techniques include timber extraction, planting and replanting of various species, cutting roads and pathways through forests, and preventing fire.\n\nThere has been an increased public awareness of natural resource policy, including forest management. Public concern regarding forest management may have shifted from the extraction of timber for earning money for the economy, to the preservation of additional forest resources, including wildlife and old growth forest, protecting biodiversity, watershed management, and recreation. Increased environmental awareness may contribute to an increased public mistrust of forest management professionals. But it can also lead to greater understanding about what professionals do re forests for nature conservation and ecological services. The importance of taking care of the forests for ecological as well as economical sustainable reasons has been shown in the TV show Ax Men.",
                    group3));
            group3.Items.Add(new SampleDataItem("Group-3-Item-2",
                    "Forest ecology",
                    "",
                    "Assets/32.png",
                    "Forest ecology is the scientific study of the interrelated patterns, processes, flora, fauna and ecosystems in forests. The management of forests is known as forestry, silviculture, and forest management.",
                    "Forest ecology is the scientific study of the interrelated patterns, processes, flora, fauna and ecosystems in forests. The management of forests is known as forestry, silviculture, and forest management. A forest ecosystem is a natural woodland unit consisting of all plants, animals and micro-organisms (Biotic components) in that area functioning together with all of the non-living physical (abiotic) factors of the environment.\n\nForest ecology is one branch of a biotically-oriented classification of types of ecological study (as opposed to a classification based on organizational level or complexity, for example population or community ecology). Thus, forests are studied at a number of organizational levels, from the individual organism to the ecosystem. However, as the term forest connotes an area inhabited by more than one organism, forest ecology most often concentrates on the level of the population, community or ecosystem. Logically, trees are an important component of forest research, but the wide variety of other life forms and abiotic components in most forests means that other elements, such as wildlife or soil nutrients, are often the focal point. Thus, forest ecology is a highly diverse and important branch of ecological study.\n\nForest ecology studies share characteristics and methodological approaches with other areas of terrestrial plant ecology. However, the presence of trees makes forest ecosystems and their study unique in numerous ways.",
                    group3));
            group3.Items.Add(new SampleDataItem("Group-3-Item-3",
                    "Old-growth forest",
                    "",
                    "Assets/33.png",
                    "An old-growth forest (also termed primary forest, virgin forest, primeval forest, late seral forest, or in Britain, ancient woodland) is a forest that has attained great age without significant disturbance.",
                    "An old-growth forest (also termed primary forest, virgin forest, primeval forest, late seral forest, or in Britain, ancient woodland) is a forest that has attained great age without significant disturbance, and thereby exhibits unique ecological features and in some cases may be classified as a climax community. Old-growth features include diversity of tree-related structures that serve as diversified wildlife habitat that leads to higher bio-diversity of the forested ecosystem. Diversified tree structure includes multi-layered canopies and canopy gaps, high variance of tree heights and diameters, diversity of decaying classes and sizes of woody debris, and diversity of tree species.\n\nOld-growth forests tend to have more large trees and standing dead trees, multi-layered canopies with gaps resulting from the deaths of individual trees, and coarse woody debris on the forest floor.\n\nForest regenerated after a severe disturbance, such as wildfire, insect infestations or harvesting, is often called second-growth or regeneration until enough time passes for the effects of the disturbance to be no longer evident. Depending on the forest, this may take anywhere from a century to several millennia. Hardwood forests of the eastern United States can develop old-growth characteristics in one or two generations of trees, or 150–500 years. In British Columbia, Canada, old growth is defined as 120 to 140 years of age in the interior of the province where fire is a frequent and natural occurrence. In British Columbia’s coastal rainforests, old growth is defined as trees more than 250 years, with some trees reaching more than 1,000 years of age. In Australia, large eucalypt trees have been radio carbon dated at around 600 years old but the understory species can be much older at around 1000 years old.",
                    group3));
            group3.Items.Add(new SampleDataItem("Group-3-Item-4",
                    "Taiga",
                    "",
                    "Assets/34.png",
                    "The taiga is the world's largest terrestrial biome. In North America it covers most of inland Canada and Alaska as well as parts of the extreme northern continental United States.",
                    "The taiga is the world's largest terrestrial biome. In North America it covers most of inland Canada and Alaska as well as parts of the extreme northern continental United States (northern Minnesota through the Upper Peninsula of Michigan to Upstate New York and northern New England) and is known as the Northwoods.\n\n In Eurasia, it covers most of Sweden, Finland, much of Norway, lowland/coastal areas of Iceland, much of Russia from Karelia in the west to the Pacific ocean (including much of Siberia), and areas of northern Kazakhstan, northern Mongolia, and northern Japan (on the island of Hokkaidō). However, the main tree species, the length of the growing season and summer temperatures vary. For example, the taiga of North America consists of mainly spruces; Scandinavian and Finnish taiga consists of a mix of spruce, pines and birch; Russian taiga has spruces, pines and larches depending on the region, the Eastern Siberian taiga being a vast larch forest. The term boreal forest is sometimes used (particularly in Canada but also in Scandinavia and Finland) to refer to the more southerly part of the biome, while the term taiga is often used to describe the more barren areas of the northernmost part of the taiga approaching the tree line and the tundra biome.",
                    group3));
            group3.Items.Add(new SampleDataItem("Group-3-Item-5",
                    "Clearcutting",
                    "",
                    "Assets/35.png",
                    "Clearcutting, or clearfelling, is a controversial forestry/logging practice in which most or all trees in an area are uniformly cut down.",
                    "Clearcutting, or clearfelling, is a controversial forestry/logging practice in which most or all trees in an area are uniformly cut down. Clearcutting, along with shelterwood and seed tree harvests, is used by foresters to create certain types of forest ecosystems and to promote select species that require an abundance of sunlight or grow in large, even-age stands. Logging companies and forest-worker unions in some countries support the practice for scientific, safety, and economic reasons. Detractors see clearcutting as synonymous with deforestation, destroying natural habitats and contributing to climate change. Clearcutting is the most popular and economically profitable method of logging. However, clearcutting also imposes other externalities in the form of detrimental side effects such as loss of topsoil; the value of these costs is intensely debated by economic, environmental, and other interests. Aside from the purpose of harvesting wood, clearcutting is also used to create land for farming.",
                    group3));
            group3.Items.Add(new SampleDataItem("Group-3-Item-6",
                    "United States Forest Service",
                    "",
                    "Assets/36.png",
                    "The United States Forest Service (USFS) is an agency of the United States Department of Agriculture that administers the nation's 155 national forests and 20 national grasslands, which encompass 193 million acres (780,000 km2). Major divisions of the agency include the National Forest System, State and Private Forestry, and the Research and Development branch.",
                    "In 1876, Congress created the office of Special Agent in the Department of Agriculture to assess the quality and conditions of forests in the United States. Franklin B. Hough was appointed the head of the office. In 1881, the office was expanded into the newly formed Division of Forestry. The Forest Reserve Act of 1891 authorized withdrawing land from the public domain as forest reserves, managed by the Department of the Interior. In 1901, the Division of Forestry was renamed the Bureau of Forestry. The Transfer Act of 1905 transferred the management of forest reserves from the General Land Office of the Interior Department to the Bureau of Forestry, henceforth known as the United States Forest Service. Gifford Pinchot was the first Chief Forester of the United States Forest Service in the administration of President Theodore Roosevelt.\n\nSignificant federal legislation affecting the Forest Service includes the Weeks Act of 1911, the Multiple Use - Sustained Yield Act of 1960, P.L. 86-517; the Wilderness Act, P.L. 88-577; the National Forest Management Act, P.L. 94-588; the National Environmental Policy Act, P.L. 91-190; the Cooperative Forestry Assistance Act, P.L. 95-313; and the Forest and Rangelands Renewable Resources Planning Act, P.L. 95-307. \n\nIn February 2009, the Government Accountability Office evaluated whether the Forest Service should be moved from the Department of Agriculture to the Department of the Interior, which already includes the National Park Service, the Fish and Wildlife Service, and the Bureau of Land Management, managing some 438,000,000 acres (1,770,000 km2) of public land.",
                    group3));
            group3.Items.Add(new SampleDataItem("Group-3-Item-7",
                    "Urban forestry",
                    "",
                    "Assets/37.png",
                    "Urban forestry is the careful care and management of urban forests, i.e., tree populations in urban settings for the purpose of improving the urban environment.",
                    "Urban forestry is the careful care and management of urban forests, i.e., tree populations in urban settings for the purpose of improving the urban environment. Urban forestry advocates the role of trees as a critical part of the urban infrastructure. Urban foresters plant and maintain trees, support appropriate tree and forest preservation, conduct research and promote the many benefits trees provide. Urban forestry is practiced by municipal and commercial arborists, municipal and utility foresters, environmental policymakers, city planners, consultants, educators, researchers and community activists.\n\nFunction, the dynamic operation of the forest, includes biochemical cycles, gas exchange, primary productivity, competition, succession, and regeneration. In urban environments, forest functions are frequently related to the human environment. Trees are usually selected, planted, trimmed, and nurtured by people, often with specific intentions, as when a tree is planted in a front yard to shade the driveway and frame the residence. The functional benefits provided by this tree depend on structural attributes, such as species and location, as well as management activities that influence its growth, crown dimensions, and health.",
                    group3));
            this.AllGroups.Add(group3);
        }
    }
}
