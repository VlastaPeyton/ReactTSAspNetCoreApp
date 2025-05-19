import React from 'react'
import { CommentGetFromBackend } from '../../Models/CommentGetFromBackend';
import StockCommentListItem from './StockCommentListItem';

type Props = {
    comments: CommentGetFromBackend[];
}

const StockCommentList = ({comments}: Props) => {
  return (
    // .map zahteva key (iako StockCommentListItem nema taj prop) kao i uvek i prosledicu index najlakse mi da bi HTML pratio elemente liste
    <>
    {comments ? comments.map((comment, index) => {return <StockCommentListItem key={index} comment={comment} />}) 
              : "" }
    </>
  )
}

export default StockCommentList;