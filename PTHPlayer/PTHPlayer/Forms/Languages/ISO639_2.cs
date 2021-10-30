using PTHPlayer.Forms.Resources;
using System.Collections.ObjectModel;
using System.Linq;

namespace PTHPlayer.Forms.Languages
{
    public static class ISO639_2
    {
        public static ISO639_2_Lang FromAlpha3(string alpha3)
        {
            Collection<ISO639_2_Lang> collection = BuildCollection();
            return collection.FirstOrDefault(p => p.Alpha3.Contains(alpha3.ToLower()));
        }

        #region Build Collection
        private static Collection<ISO639_2_Lang> BuildCollection()
        {
            Collection<ISO639_2_Lang> collection = new Collection<ISO639_2_Lang>();

            collection.Add(new ISO639_2_Lang(ISO639Resources.Afar, "aar"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.Abkhaz, "abk"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.Avestan, "ave"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.Afrikaans, "afr"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.Akan, "aka"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.Amharic, "amh"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.Aragonese, "arg"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.Arabic, "ara"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.Assamese, "asm"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.Avaric, "ava"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.Aymara, "aym"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.Azerbaijani, "aze"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.Bashkir, "bak"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.Belarusian, "bel"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.Bulgarian, "bul"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.Bihari, "bih"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.Bislama, "bis"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.Bambara, "bam"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.Bengali, "ben"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.Tibetan, "tib/bod"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.Breton, "bre"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.Bosnian, "bos"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.Catalan, "cat"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.Chechen, "che"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.Chamorro, "cha"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.Corsican, "cos"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.Cree, "cre"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.Czech, "cze/ces"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.ChurchSlavic, "chu"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.Chuvash, "chv"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.Welsh, "wel/cym"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.Danish, "dan"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.German, "ger/deu"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.Divehi, "div"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.Dzongkha, "dzo"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.Ewe, "ewe"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.Greek, "gre/ell"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.English, "eng"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.Esperanto, "epo"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.Spanish, "spa"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.Estonian, "est"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.Basque, "baq/eus"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.Persian, "per/fas"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.Fulah, "ful"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.Finnish, "fin"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.Fijian, "fij"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.Faroese, "fao"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.French, "fre/fra"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.WesternFrisian, "fry"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.Irish, "gle"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.Gaelic, "gla"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.Galician, "glg"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.Guarani, "grn"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.Gujarati, "guj"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.Manx, "glv"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.Hausa, "hau"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.Hebrew, "heb"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.Hindi, "hin"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.HiriMotu, "hmo"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.Croatian, "scr/hrv"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.Haitian, "hat"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.Hungarian, "hun"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.Armenian, "arm/hye"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.Herero, "her"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.Interlingua, "ina"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.Indonesian, "ind"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.Interlingue, "ile"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.Igbo, "ibo"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.SichuanYi, "iii"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.Inupiaq, "ipk"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.Ido, "ido"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.Icelandic, "ice/isl"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.Italian, "ita"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.Inuktitut, "iku"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.Japanese, "jpn"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.Javanese, "jav"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.Georgian, "geo/kat"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.Kongo, "kon"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.Kikuyu, "kik"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.Kuanyama, "kua"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.Kazakh, "kaz"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.Kalaallisut, "kal"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.Khmer, "khm"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.Kannada, "kan"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.Korean, "kor"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.Kanuri, "kau"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.Kashmiri, "kas"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.Kurdish, "kur"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.Komi, "kom"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.Cornish, "cor"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.Kirghiz, "kir"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.Latin, "lat"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.Luxembourgish, "ltz"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.Ganda, "lug"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.Limburgish, "lim"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.Lingala, "lin"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.Lao, "lao"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.Lithuanian, "lit"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.LubaKatanga, "lub"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.Latvian, "lav"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.Malagasy, "mlg"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.Marshallese, "mah"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.Maori, "mao/mri"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.Macedonian, "mac/mkd"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.Malayalam, "mal"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.Mongolian, "mon"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.Moldavian, "mol"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.Marathi, "mar"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.Malay, "may/msa"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.Maltese, "mlt"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.Burmese, "bur/mya"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.Nauru, "nau"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.NorwegianBokmal, "nob"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.NorthNdebele, "nde"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.Nepali, "nep"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.Ndonga, "ndo"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.Dutch, "dut/nld"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.NorwegianNynorsk, "nno"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.Norwegian, "nor"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.SouthNdebele, "nbl"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.Navajo, "nav"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.Chichewa, "nya"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.Occitan, "oci"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.Ojibwa, "oji"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.Oromo, "orm"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.Oriya, "ori"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.Ossetian, "oss"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.Panjabi, "pan"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.Pali, "pli"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.Polish, "pol"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.Pashto, "pus"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.Portuguese, "por"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.Quechua, "que"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.RaetoRomance, "roh"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.Kirundi, "run"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.Romanian, "rum/ron"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.Russian, "rus"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.Kinyarwanda, "kin"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.Sanskrit, "san"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.Sardinian, "srd"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.Sindhi, "snd"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.NorthernSami, "sme"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.Sango, "sag"));
            //collection.Add(new ISO639_2_Lang(ISO639Resources.SerboCroatian, "aar"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.Sinhala, "sin"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.Slovak, "slo/slk"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.Slovenian, "slv"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.Samoan, "smo"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.Shona, "sna"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.Somali, "som"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.Albanian, "alb/sqi"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.Serbian, "scc/srp"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.Swati, "ssw"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.SouthernSotho, "sot"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.Sundanese, "sun"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.Swedish, "swe"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.Swahili, "swa"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.Tamil, "tam"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.Telugu, "tel"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.Tajik, "tgk"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.Thai, "tha"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.Tigrinya, "tir"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.Turkmen, "tuk"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.Tagalog, "tgl"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.Tswana, "tsn"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.Tonga, "ton"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.Turkish, "tur"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.Tsonga, "tso"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.Tatar, "tat"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.Twi, "twi"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.Tahitian, "tah"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.Uighur, "uig"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.Ukrainian, "ukr"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.Urdu, "urd"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.Uzbek, "uzb"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.Venda, "ven"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.Vietnamese, "vie"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.Volapuk, "vol"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.Walloon, "wln"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.Wolof, "wol"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.Xhosa, "xho"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.Yiddish, "yid"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.Yoruba, "yor"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.Zhuang, "zha"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.Chinese, "chi/zho"));
            collection.Add(new ISO639_2_Lang(ISO639Resources.Zulu, "zul"));

            return collection;
        }
        #endregion
    }

    public class ISO639_2_Lang
    {
        public ISO639_2_Lang(string name, string alpha3)
        {
            this.Name = name;
            this.Alpha3 = alpha3;
        }

        public string Name { get; private set; }
        public string Alpha3 { get; private set; }
    }
}
