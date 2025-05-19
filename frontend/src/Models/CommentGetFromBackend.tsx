export type CommentGetFromBackend = {
    title: string;
    content: string;
    createdBy: string;
    // Nisam izabravo vise polja iz CommentDTO jer mi ne trebaju.
}

/* U Backend CommentController GetAll Endpoint vraca List of CommentDTO, a ja samo biram ova 3 polja jer mi samo ona trebaju.
 Automatski se mapira Title->title, Content -> content i CreatedBy -> createdBy iz Backend to here. */