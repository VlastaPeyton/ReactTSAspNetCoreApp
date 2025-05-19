import React, { useState, MouseEvent, SyntheticEvent, ChangeEvent } from 'react'

// Ovako se prosledjuje funkcija u Props
type Props = {
    onSearchSubmit: (e: SyntheticEvent) => void;
    search: string | undefined;
    handleSearchChange: (eTargetValue: string) => void;
}

const Search = ({onSearchSubmit, search, handleSearchChange}: Props) => {
   
    return (
        /* In <form>, onSubmit function (onSearchSubmit) mora imati e.preventDefault() in it's definition, 
         dok kod onChange ili onClick (u <form> ili van forme), skoro nikad nema e.preventDefault(), stoga u handleSearchChange 
         nece biti e.preventDefault, jer handleSearchChange ne prima argument tipa Event, vec string koji sam uneo u formu.  */
        <div className="flex justify-center items-center my-10 px-4">
            <form
                onSubmit={(e) => onSearchSubmit(e)}
                className="flex flex-col sm:flex-row gap-4 bg-white shadow-lg rounded-lg p-6 w-full max-w-2xl"
            >
                <input
                    value={search}
                    onChange={(e) => handleSearchChange(e.target.value)}
                    placeholder="Enter ticker, not a real name"
                    className="flex-1 px-4 py-3 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-green-400 text-lg"
                />
                <button
                    type="submit"
                    className="bg-green-500 hover:bg-green-600 text-white font-semibold px-6 py-3 rounded-md text-lg transition duration-300"
                >
                    Search {/* Kliknem na ovo pokrecem onSearchSubmit u SearchPage.tsx */}
                 </button>
            </form>
       </div>
    )
}

export default Search;