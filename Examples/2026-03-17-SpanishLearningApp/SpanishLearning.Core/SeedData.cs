using SpanishLearning.Core.Models;

namespace SpanishLearning.Core;

public static class SeedData
{
    public static readonly IReadOnlyList<Flashcard> Flashcards = new List<Flashcard>
    {
        // Greetings
        new(1, "Hola", "Hello", "greetings"),
        new(2, "Buenos días", "Good morning", "greetings"),
        new(3, "Buenas tardes", "Good afternoon", "greetings"),
        new(4, "Buenas noches", "Good night", "greetings"),
        new(5, "Adiós", "Goodbye", "greetings"),
        new(6, "Por favor", "Please", "greetings"),
        new(7, "Gracias", "Thank you", "greetings"),
        new(8, "De nada", "You're welcome", "greetings"),

        // Numbers
        new(9, "Uno", "One", "numbers"),
        new(10, "Dos", "Two", "numbers"),
        new(11, "Tres", "Three", "numbers"),
        new(12, "Cuatro", "Four", "numbers"),
        new(13, "Cinco", "Five", "numbers"),
        new(14, "Diez", "Ten", "numbers"),
        new(15, "Veinte", "Twenty", "numbers"),

        // Food
        new(16, "El pan", "Bread", "food"),
        new(17, "La leche", "Milk", "food"),
        new(18, "El agua", "Water", "food"),
        new(19, "La manzana", "Apple", "food"),
        new(20, "El pollo", "Chicken", "food"),
        new(21, "El arroz", "Rice", "food"),
        new(22, "La sopa", "Soup", "food"),
    };

    public static readonly IReadOnlyList<QuizQuestion> QuizQuestions = new List<QuizQuestion>
    {
        new(1, "What does 'Hola' mean?",
            ["Hello", "Goodbye", "Please", "Thank you"], 0,
            "'Hola' is the most common Spanish greeting, equivalent to 'Hello'."),

        new(2, "How do you say 'Thank you' in Spanish?",
            ["Por favor", "De nada", "Gracias", "Adiós"], 2,
            "'Gracias' means 'Thank you'. 'De nada' means 'You're welcome'."),

        new(3, "What is the Spanish word for 'Bread'?",
            ["El arroz", "El pan", "La leche", "El agua"], 1,
            "'El pan' is bread. Note that most food nouns in Spanish have a definite article (el/la)."),

        new(4, "What does 'Dos' mean?",
            ["One", "Three", "Two", "Four"], 2,
            "'Dos' is the number two in Spanish."),

        new(5, "Which greeting is used in the morning?",
            ["Buenas noches", "Buenas tardes", "Adiós", "Buenos días"], 3,
            "'Buenos días' is used in the morning. 'Buenas tardes' is for the afternoon, 'Buenas noches' for the evening/night."),

        new(6, "What is the correct present tense of 'hablar' (to speak) for 'yo' (I)?",
            ["hablas", "hablo", "habla", "hablamos"], 1,
            "In the present tense, 'yo' + -ar verbs take the ending -o: yo hablo."),

        new(7, "Which sentence means 'I eat an apple'?",
            ["Yo bebo agua", "Yo como una manzana", "Yo hablo español", "Yo leo un libro"], 1,
            "'Yo como' = 'I eat', 'una manzana' = 'an apple'."),

        new(8, "What does 'Veinte' mean?",
            ["Ten", "Fifteen", "Five", "Twenty"], 3,
            "'Veinte' is twenty. 'Diez' is ten."),

        new(9, "How do you say 'Water' in Spanish?",
            ["La leche", "El jugo", "El agua", "La sopa"], 2,
            "'El agua' means water. Even though 'agua' is feminine, it uses 'el' to avoid two vowel sounds together."),

        new(10, "What is the present tense of 'comer' (to eat) for 'tú' (you)?",
            ["como", "come", "comes", "comemos"], 2,
            "For -er verbs, the 'tú' ending in the present tense is -es: tú comes."),
    };

    public static readonly IReadOnlyList<Story> Stories = new List<Story>
    {
        new(1,
            "Un Día en el Mercado (A Day at the Market)",
            """
            María va al mercado por la mañana. Ella quiere comprar frutas y verduras para su familia. El mercado está lleno de colores y aromas.

            Ella ve manzanas rojas, plátanos amarillos y naranjas brillantes. También hay tomates frescos y zanahorias crujientes. María habla con el vendedor: "Buenos días. ¿Cuánto cuestan las manzanas?"

            El vendedor responde: "Un euro por kilo, señora." María compra dos kilos de manzanas y un kilo de tomates. Ella paga y dice: "Muchas gracias." El vendedor sonríe y dice: "De nada. ¡Buen día!"
            """,
            """
            María goes to the market in the morning. She wants to buy fruits and vegetables for her family. The market is full of colors and aromas.

            She sees red apples, yellow bananas, and bright oranges. There are also fresh tomatoes and crunchy carrots. María talks to the vendor: "Good morning. How much do the apples cost?"

            The vendor replies: "One euro per kilo, ma'am." María buys two kilos of apples and one kilo of tomatoes. She pays and says: "Thank you very much." The vendor smiles and says: "You're welcome. Have a nice day!"
            """
        ),
        new(2,
            "El Viaje a Madrid (The Trip to Madrid)",
            """
            Carlos y su amiga Ana deciden viajar a Madrid durante el fin de semana. Reservan un hotel en el centro de la ciudad y compran billetes de tren con anticipación.

            Al llegar, visitan el Museo del Prado, donde admiran obras de Velázquez y Goya. Carlos explica a Ana la historia detrás de cada cuadro porque ha estudiado arte. Ana está fascinada por la riqueza cultural del museo.

            Por la tarde, pasean por el Parque del Retiro y alquilan un bote de remos en el estanque. El sol se pone mientras regresan al hotel, cansados pero muy contentos. "Definitivamente tenemos que volver el año que viene," dice Ana. Carlos sonríe y responde: "Completamente de acuerdo."
            """,
            """
            Carlos and his friend Ana decide to travel to Madrid for the weekend. They book a hotel in the city center and buy train tickets in advance.

            Upon arriving, they visit the Prado Museum, where they admire works by Velázquez and Goya. Carlos explains the history behind each painting to Ana because he has studied art. Ana is fascinated by the museum's cultural richness.

            In the afternoon, they stroll through Retiro Park and rent a rowing boat on the pond. The sun sets as they return to the hotel, tired but very happy. "We definitely have to come back next year," says Ana. Carlos smiles and replies: "Completely agree."
            """
        ),
    };

    public static readonly IReadOnlyList<Lesson> Lessons = new List<Lesson>
    {
        new(1, "Basic Greetings", new LessonSection[]
        {
            new("Introduction", "Greetings are the foundation of any conversation. In Spanish, the time of day matters when choosing your greeting."),
            new("Common Greetings",
                "• Hola – Hello (used any time of day)\n• Buenos días – Good morning (until around noon)\n• Buenas tardes – Good afternoon (noon until sunset)\n• Buenas noches – Good evening/night (after sunset)"),
            new("Polite Expressions",
                "• Por favor – Please\n• Gracias – Thank you\n• De nada – You're welcome\n• Perdón / Disculpe – Excuse me / Sorry"),
            new("Saying Goodbye",
                "• Adiós – Goodbye\n• Hasta luego – See you later\n• Hasta mañana – See you tomorrow\n• Nos vemos – See you"),
            new("Practice", "Try greeting someone using the appropriate time-of-day greeting. Then practice a mini conversation: introduce yourself and say goodbye."),
        }),
        new(2, "Numbers 1–20", new LessonSection[]
        {
            new("Introduction", "Learning numbers is essential for shopping, telling time, and everyday tasks. Let's learn 1 through 20."),
            new("Numbers 1–10",
                "1 – Uno\n2 – Dos\n3 – Tres\n4 – Cuatro\n5 – Cinco\n6 – Seis\n7 – Siete\n8 – Ocho\n9 – Nueve\n10 – Diez"),
            new("Numbers 11–20",
                "11 – Once\n12 – Doce\n13 – Trece\n14 – Catorce\n15 – Quince\n16 – Dieciséis\n17 – Diecisiete\n18 – Dieciocho\n19 – Diecinueve\n20 – Veinte"),
            new("Tips", "Numbers 16–19 are contractions of 'diez y seis', 'diez y siete', etc. Notice the accent on dieciséis (16)."),
            new("Practice", "Count from 1 to 20 aloud. Then try to say your age, your phone number, and the number of people in your household in Spanish."),
        }),
        new(3, "Present Tense Verbs", new LessonSection[]
        {
            new("Introduction", "The present tense (presente de indicativo) is used to describe current actions, habits, and general truths. Spanish verbs are grouped by their infinitive endings: -ar, -er, and -ir."),
            new("-AR Verb Conjugation",
                "Example: hablar (to speak)\n• Yo hablo – I speak\n• Tú hablas – You speak\n• Él/Ella habla – He/She speaks\n• Nosotros hablamos – We speak\n• Vosotros habláis – You all speak (Spain)\n• Ellos/Ellas hablan – They speak"),
            new("-ER Verb Conjugation",
                "Example: comer (to eat)\n• Yo como – I eat\n• Tú comes – You eat\n• Él/Ella come – He/She eats\n• Nosotros comemos – We eat\n• Vosotros coméis – You all eat (Spain)\n• Ellos/Ellas comen – They eat"),
            new("-IR Verb Conjugation",
                "Example: vivir (to live)\n• Yo vivo – I live\n• Tú vives – You live\n• Él/Ella vive – He/She lives\n• Nosotros vivimos – We live\n• Vosotros vivís – You all live (Spain)\n• Ellos/Ellas viven – They live"),
            new("Common Irregular Verbs",
                "Some very common verbs are irregular:\n• Ser (to be): soy, eres, es, somos, sois, son\n• Tener (to have): tengo, tienes, tiene, tenemos, tenéis, tienen\n• Ir (to go): voy, vas, va, vamos, vais, van"),
            new("Practice", "Conjugate these verbs in all persons: trabajar (to work), beber (to drink), escribir (to write). Then write 3 sentences about your daily routine using present tense verbs."),
        }),
    };

    public static readonly IReadOnlyList<Resource> Resources = new List<Resource>
    {
        new(1, "Duolingo", "https://www.duolingo.com", "Free gamified language learning app with Spanish courses for all levels."),
        new(2, "SpanishPod101", "https://www.spanishpod101.com", "Audio and video lessons covering Spanish grammar, vocabulary, and culture."),
        new(3, "Real Academia Española", "https://www.rae.es", "Official dictionary of the Spanish language from the Royal Spanish Academy."),
        new(4, "Conjuguemos", "https://conjuguemos.com", "Interactive verb conjugation practice tool covering all Spanish tenses."),
        new(5, "Forvo", "https://forvo.com/languages/es/", "Pronunciation dictionary with native speaker audio recordings for Spanish words."),
        new(6, "FluentU", "https://www.fluentu.com/blog/spanish/", "Blog with tips, vocabulary lists, and resources for Spanish learners."),
        new(7, "Language Transfer: Spanish", "https://www.languagetransfer.org/spanish", "Free audio course that teaches Spanish by connecting it to English structures."),
    };
}
