using System;
using System.Linq;
using System.Text;
using UnityEngine;

namespace BlockYourFriends.Multiplayer
{
    /// <summary>
    /// Just for fun, give a cute default player name if no name is provided, based on a hash of their anonymous ID.
    /// </summary>
    public static class NameGenerator
    {
        private static string[] AIMaleFirstNames = { "James", "Robert", "John", "Michael", "William", "David", "Richard", "Joseph", "Thomas", "Charles", "Christopher", "Daniel", "Matthew", "Anthony", "Mark", "Donald", "Steven", "Paul", "Andrew", "Joshua", "Kenneth", "Kevin", "Brian", "George", "Edward", "Ronald", "Timothy", "Jason", "Jeffrey", "Ryan", "Jacob", "Gary", "Nicholas", "Eric", "Jonathan", "Stephen", "Larry", "Justin", "Scott", "Brandon", "Benjamin", "Samuel", "Gregory", "Frank", "Alexander", "Raymond", "Patrick", "Jack", "Dennis", "Jerry", "Tyler", "Aaron", "Jose", "Adam", "Henry", "Nathan", "Douglas", "Zachary", "Peter", "Kyle", "Walter", "Ethan", "Jeremy", "Harold", "Keith", "Christian", "Roger", "Noah", "Gerald", "Carl", "Terry", "Sean", "Austin", "Arthur", "Lawrence", "Jesse", "Dylan", "Bryan", "Joe", "Jordan", "Billy", "Bruce", "Albert", "Willie", "Gabriel", "Logan", "Alan", "Juan", "Wayne", "Roy", "Ralph", "Randy", "Eugene", "Vincent", "Russell", "Elijah", "Louis", "Bobby", "Philip", "Johnny" };
        private static string[] AIFemaleFirstNames = { "Mary", "Patricia", "Jennifer", "Linda", "Elizabeth", "Barbara", "Susan", "Jessica", "Sarah", "Karen", "Nancy", "Lisa", "Betty", "Margaret", "Sandra", "Ashley", "Kimberly", "Emily", "Donna", "Michelle", "Dorothy", "Carol", "Amanda", "Melissa", "Deborah", "Stephanie", "Rebecca", "Sharon", "Laura", "Cynthia", "Kathleen", "Amy", "Shirley", "Angela", "Helen", "Anna", "Brenda", "Pamela", "Nicole", "Emma", "Samantha", "Katherine", "Christine", "Debra", "Rachel", "Catherine", "Carolyn", "Janet", "Ruth", "Maria", "Heather", "Diane", "Virginia", "Julie", "Joyce", "Victoria", "Olivia", "Kelly", "Christina", "Lauren", "Joan", "Evelyn", "Judith", "Megan", "Cheryl", "Andrea", "Hannah", "Martha", "Jacqueline", "Frances", "Gloria", "Ann", "Teresa", "Kathryn", "Sara", "Janice", "Jean", "Alice", "Madison", "Doris", "Abigail", "Julia", "Judy", "Grace", "Denise", "Amber", "Marilyn", "Beverly", "Danielle", "Theresa", "Sophia", "Marie", "Diana", "Brittany", "Natalie", "Isabella", "Charlotte", "Rose", "Alexis", "Kayla" };
        private static string[] AILastNames = { "Smith", "Johnson", "Williams", "Brown", "Jones", "Garcia", "Miller", "Davis", "Rodriguez", "Martinez", "Hernandez", "Lopez", "Gonzales", "Wilson", "Anderson", "Thomas", "Taylor", "Moore", "Jackson", "Martin", "Lee", "Perez", "Thompson", "White", "Harris", "Sanchez", "Clark", "Ramirez", "Lewis", "Robinson", "Walker", "Young", "Allen", "King", "Wright", "Scott", "Torres", "Nguyen", "Hill", "Flores", "Green", "Adams", "Nelson", "Baker", "Hall", "Rivera", "Campbell", "Mitchell", "Carter", "Roberts", "Gomez", "Phillips", "Evans", "Turner", "Diaz", "Parker", "Cruz", "Edwards", "Collins", "Reyes", "Stewart", "Morris", "Morales", "Murphy", "Cook", "Rogers", "Gutierrez", "Ortiz", "Morgan", "Cooper", "Peterson", "Bailey", "Reed", "Kelly", "Howard", "Ramos", "Kim", "Cox", "Ward", "Richardson", "Watson", "Brooks", "Chavez", "Wood", "James", "Bennet", "Gray", "Mendoza", "Ruiz", "Hughes", "Price", "Alvarez", "Castillo", "Sanders", "Patel", "Myers", "Long", "Ross", "Foster", "Jimenez" };

        public static string GetName(string userId)
        {
            if (PlayerPrefs.HasKey(userId))
            {
                string savedName = PlayerPrefs.GetString(userId);

                if (!string.IsNullOrEmpty(savedName))
                    return savedName;
            }

            int seed = userId.GetHashCode();
            seed *= Math.Sign(seed);
            StringBuilder nameOutput = new StringBuilder();
            #region Word part
            int word = seed % 88;
            if (word == 0) nameOutput.Append("Andromeda");
            else if (word == 1) nameOutput.Append("Antlia");
            else if (word == 2) nameOutput.Append("Apus");
            else if (word == 3) nameOutput.Append("Aquarius");
            else if (word == 4) nameOutput.Append("Aquila");
            else if (word == 5) nameOutput.Append("Ara");
            else if (word == 6) nameOutput.Append("Aries");
            else if (word == 7) nameOutput.Append("Auriga");
            else if (word == 8) nameOutput.Append("Bootes");
            else if (word == 9) nameOutput.Append("Caelum");
            else if (word == 10) nameOutput.Append("Camelopardalis");
            else if (word == 11) nameOutput.Append("Cancer");
            else if (word == 12) nameOutput.Append("Venatici");
            else if (word == 13) nameOutput.Append("CanisMajor");
            else if (word == 14) nameOutput.Append("CanisMinor");
            else if (word == 15) nameOutput.Append("Capricornus");
            else if (word == 16) nameOutput.Append("Carina");
            else if (word == 17) nameOutput.Append("Cassiopeia");
            else if (word == 18) nameOutput.Append("Centaurus");
            else if (word == 19) nameOutput.Append("Cepheus");
            else if (word == 20) nameOutput.Append("Cetus");
            else if (word == 21) nameOutput.Append("Chamaeleon");
            else if (word == 22) nameOutput.Append("Circinus");
            else if (word == 23) nameOutput.Append("Columba");
            else if (word == 24) nameOutput.Append("Berenices");
            else if (word == 25) nameOutput.Append("Australis");
            else if (word == 26) nameOutput.Append("Borealis");
            else if (word == 27) nameOutput.Append("Corvus");
            else if (word == 28) nameOutput.Append("Crater");
            else if (word == 29) nameOutput.Append("Crux");
            else if (word == 30) nameOutput.Append("Cygnus");
            else if (word == 31) nameOutput.Append("Delphinus");
            else if (word == 32) nameOutput.Append("Dorado");
            else if (word == 33) nameOutput.Append("Draco");
            else if (word == 34) nameOutput.Append("Equuleus");
            else if (word == 35) nameOutput.Append("Eridanus");
            else if (word == 36) nameOutput.Append("Fornax");
            else if (word == 37) nameOutput.Append("Gemini");
            else if (word == 38) nameOutput.Append("Grus");
            else if (word == 39) nameOutput.Append("Hercules");
            else if (word == 40) nameOutput.Append("Horologium");
            else if (word == 41) nameOutput.Append("Hydra");
            else if (word == 42) nameOutput.Append("Hydrus");
            else if (word == 43) nameOutput.Append("Indus");
            else if (word == 44) nameOutput.Append("Lacerta");
            else if (word == 45) nameOutput.Append("Leo");
            else if (word == 46) nameOutput.Append("LeoMinor");
            else if (word == 47) nameOutput.Append("Lepus");
            else if (word == 48) nameOutput.Append("Libra");
            else if (word == 49) nameOutput.Append("Lupus");
            else if (word == 50) nameOutput.Append("Lynx");
            else if (word == 51) nameOutput.Append("Lyra");
            else if (word == 52) nameOutput.Append("Mensa");
            else if (word == 53) nameOutput.Append("Microscopium");
            else if (word == 54) nameOutput.Append("Monoceros");
            else if (word == 55) nameOutput.Append("Musca");
            else if (word == 56) nameOutput.Append("Norma");
            else if (word == 57) nameOutput.Append("Octans");
            else if (word == 58) nameOutput.Append("Ophiuchus");
            else if (word == 59) nameOutput.Append("Orion");
            else if (word == 60) nameOutput.Append("Pavo");
            else if (word == 61) nameOutput.Append("Pegasus");
            else if (word == 62) nameOutput.Append("Perseus");
            else if (word == 63) nameOutput.Append("Phoenix");
            else if (word == 64) nameOutput.Append("Pictor");
            else if (word == 65) nameOutput.Append("Pisces");
            else if (word == 66) nameOutput.Append("Piscis");
            else if (word == 67) nameOutput.Append("Puppis");
            else if (word == 68) nameOutput.Append("Pyxis");
            else if (word == 69) nameOutput.Append("Reticulum");
            else if (word == 70) nameOutput.Append("Sagitta");
            else if (word == 71) nameOutput.Append("Sagittarius");
            else if (word == 72) nameOutput.Append("Scorpius");
            else if (word == 73) nameOutput.Append("Sculptor");
            else if (word == 74) nameOutput.Append("Scutum");
            else if (word == 75) nameOutput.Append("Serpens");
            else if (word == 76) nameOutput.Append("Sextans");
            else if (word == 77) nameOutput.Append("Taurus");
            else if (word == 78) nameOutput.Append("Telescopium");
            else if (word == 79) nameOutput.Append("Triangulum");
            else if (word == 80) nameOutput.Append("Australe");
            else if (word == 81) nameOutput.Append("Tucana");
            else if (word == 82) nameOutput.Append("UrsaMajor");
            else if (word == 83) nameOutput.Append("UrsaMinor");
            else if (word == 84) nameOutput.Append("Vela");
            else if (word == 85) nameOutput.Append("Virgo");
            else if (word == 86) nameOutput.Append("Volans");
            else nameOutput.Append("Vulpecula");
            #endregion

            int number = seed % 1000;
            nameOutput.Append(number.ToString("000"));

            return nameOutput.ToString();
        }

        public static string GetRandomNameForAI()
        {
            StringBuilder randomName = new StringBuilder();

            int randomSex = UnityEngine.Random.Range(0, 2);
            if (randomSex == 0)
            {
                int randomFirstNameIndex = UnityEngine.Random.Range(0, AIMaleFirstNames.Length);
                randomName.Append(AIMaleFirstNames[randomFirstNameIndex]);
            }
            else
            {
                int randomFirstNameIndex = UnityEngine.Random.Range(0, AIFemaleFirstNames.Length);
                randomName.Append(AIFemaleFirstNames[randomFirstNameIndex]);
            }

            randomName.Append(" ");

            int randomLastNameIndex = UnityEngine.Random.Range(0, AILastNames.Length);
            randomName.Append(AILastNames[randomLastNameIndex]);

            return randomName.ToString();
        }
    }
}