export type UserProfileToken = {
    userName :string;
    emailAddress: string;
    token: string; // JWT
}
// Register i Login method u Backend vraca objekat sa UserName, Email, Token parametrima, ali ga automatski mapira na ove nase jer su imena "ista"