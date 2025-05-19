import React from 'react'
import { PortfolioGetFromBackend } from '../../../Models/PortfolioGetFromBackend';

type Props = {
    index: number;
    portfolioSymbol: string;
    onPortfolioDelete: (symbol: string) => void;
};

const DeletePortfolio = ({index, portfolioSymbol, onPortfolioDelete}: Props) => {
    /* U SearchPage.tsx onPortfolioDelete nema e.preventDefault(), jer je onPortfolioDelete vezan za onClick i pritom nije forma, stoga
    ne prosledjujemo event i ne brinem da l ce da reload sve jer nece. */
    return (
        <button className="bg-red-600 text-white px-3 py-1 rounded hover:bg-red-700 transition" onClick={() => onPortfolioDelete(portfolioSymbol)}>X</button>
    )
}

export default DeletePortfolio;