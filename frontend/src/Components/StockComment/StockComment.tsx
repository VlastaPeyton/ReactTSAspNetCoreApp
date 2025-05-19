import React, { useEffect, useState } from 'react'
import StockCommentForm from './StockCommentForm/StockCommentForm';
import { commentPostAPI, commentsGetAPI } from '../../Services/CommentService';
import { toast } from 'react-toastify';
import { useAuth } from '../../Context/useAuthContext';
import { CommentGetFromBackend } from '../../Models/CommentGetFromBackend';
import Spinner from '../Spinner/Spinner';
import StockCommentListItem from '../StockCommentList/StockCommentListItem';
import StockCommentList from '../StockCommentList/StockCommentList';

type Props = {
  stockSymbol: string;
}

type CommentFormInput = {
  title: string;
  content: string;
}

const StockComment = ({stockSymbol}: Props) => {

  const [comments, setComments] = useState<CommentGetFromBackend[] | null>(null);
  const [loading, setLoading] = useState<boolean>();

  useEffect(() => {
    getComments();
  },[]) // Only when loading page to load comments

  const handleComment = (input: CommentFormInput) => {
    commentPostAPI(input.title, input.content, stockSymbol).then((result) => {
      if (result){
        toast.success("Comment created uspesno");
        getComments(); // Da vidimo auziranu listu kad dodamo na licu mesta novi
      }
    }).catch((err) => {
      toast.warning(err);
    })
  }

  const getComments = () => {
    setLoading(true);
    commentsGetAPI(stockSymbol).then((result) => {
      setLoading(false);
      setComments(result?.data!); // Mora !
    })
  }

  return (
    <div className="flex flex-col">
      {loading ? <Spinner /> : <StockCommentList comments={comments!} /> }
      <StockCommentForm symbol={stockSymbol}  handleComment={handleComment}/> 
    </div>
  )
}

export default StockComment;