export type UserProfile = {
    userName: string;
    emailAddress: string;
}

// Login method u Backend vraca objekat sa UserName, Email  parametrima, ali ga automatski mapira na ove nase jer su imena "ista"