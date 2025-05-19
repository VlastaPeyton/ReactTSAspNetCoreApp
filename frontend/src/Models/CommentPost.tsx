export type CommentPost = {
    title: string;
    content: string;
}
/* Endpoint https://localhost:port/api/comment/{symbol} Endpoint  u Backend vraca  return CreatedAtAction(nameof(GetById), new { id = comment.Id }, comment.ToCommentDTO()); 
tj u Frontend vraca samo comment.ToCommentDTO() objekat tj CommentDTO objekat, a meni samo trebaju Title i Content iz njega.
Autoamtski se mapira Title->title i Content->content from Backend to here. */