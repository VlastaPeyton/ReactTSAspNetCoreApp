import axios from "axios";
import { CompanyBalanceSheet, CompanyCashFlow, CompanyIncomeStatement, CompanyKeyMetrics, CompanyProfile, CompanySearch, CompanyTenK } from "../company"; // Jer nema default export u company.d.ts, pa mora {}
import { SearchResponse } from "../Models/SearchResponse";

// interface SearchResponse {
//     data: CompanySearch[]; // Niz elemanata of CompanySearch type jer searchCompanies API vraca niz
// } ovo sam u Models stavio.

// Obzirom da exportujem >= 2 stvari, moram ovako exportovati na mestu definisanja. 

// Axios korisitm umesto Fetch, jer je bolji i more user-friendly than Fetch.

// Obzirom da ovde gadjam FinancialModelingPrep API koji je public na netu tj nisam ga ja pravio, u catch bloku ne koristim handleError jer je ta funkcija pravljena za moj licni backend.

// For SearchPage.tsx
/*Kada u search ukucam npr "aap" dobijem sve firme koje u imenu ticker imaju "aap", ali firma moze biti Stock ili ETF. Ovaj API ne garantuje da je Stock. 
zato ako ne mogu da dodam npr AAPU u my portfolio jer Endpoint, koga portfolioAddApi funkcija gadja, koristi drugaciji FMP API koji samo Stock pretrazuje. Sto znaci
da AAPU nije Stock nego ETF pa necemo moci da ga dodamo u portfolio.*/
export const searchCompanies = async (query: string) => {
    // try-catch zbog axios. Try se izvrsi ako StatusCode=2XX from backend. Catch se izvrsi ako StatusCode!=2XX from backend.
    try{
        // Nije potrebno JWT slati, jer ovo je API sa neta i public je. 
        // Moram navesti <SearchResponse> kao POTENCIJALNI return ako backend ne vrati error, jer uspesan ovaj Endpoint sa FinancialModelingPrep vraca niz ComapnySearch tipa, a meni trebaju sva polja iz CompanySearch - objasnjeno u SearcResponse. 
        // Da mi treba samo jedan element, moram opet <SearchResponse>, pa onda response.data[i] za zelejeni element.
        const response = await axios.get<SearchResponse>(`https://financialmodelingprep.com/api/v3/search-ticker?query=${query}&limit=10&exchange=NASDAQ&apikey=${process.env.REACT_APP_API_KEY}`); 
        //const response = await axios.get<CompanySearch[]>(`https://financialmodelingprep.com/api/v3/search-ticker?query=${query}&limit=10&exchange=NASDAQ&apikey=${process.env.REACT_APP_API_KEY}`);  Ovo je lakse jer mi trebaju sva polja iz CompanySearch
        // U Axios GET Request, samo Header moze, a Body ne moze. Nemam header, jer mi header tj JWT ne treba, jer ovo je public API sa neta.
        
        return response; // Full response AxiosResponse<SearchResponse> tipa ili undefinded ako backend Endpoint vrati error jer onda idem u catch block.
                         // Zbog full response ovako (stavio sam namerno bez response.data ovde), da bih u SearchPage.tsx onSearchSubmit uradio result.data
       
    } catch (err){
        /* Obzirom da ovde gadjam FinancialModelingPrep API koji je public na netu tj nisam ga ja pravio, u catch bloku ne koristim handleError jer je ta funkcija pravljena za moj licni backend. 
        vec koristim obican isAxiosError koji moze biti Network/HTTP/Unexpected . */
        
        if (axios.isAxiosError(err)){ // HTTP/Network error imaju err.message 
            console.log("Err message:", err.message)
            return err.message;       // String je ovo - bitno zbog SearchPage onSearchSubmit kad handlujem result
        } 
        else{                         // Unexpected error koji nema err.message
            console.log("Unexpected error", err);
            return "Unexpected error";  // String je ovo - bitno zbog SearchPage onSearchSubmit kad handlujem result
        }
        // Zbog return string u catch, u SearchPage.tsx onSearchSubmit moram da proverim typeof(return) == "string", a zbog try provera Array.isArray(return.data)... 
    }
};

// For CompanyPage.tsx
export const getCompanyProfile = async (query: string) => {
    // try-catch zbog axios. Try se izvrsi ako StatusCode=2XX from backend. Catch se izvrsi ako StatusCode!=2XX from backend.
    try{
        // Nije potrebno JWT slati, jer ovo je API sa neta i public je. 
        // Moram navesti <CompanyProfile[]> kao POTENCIJALNI return ako backend ne vrti error, jer uspesan ovaj Endpoint sa FinancialModelingPrep vraca niz CompanyProfile tipa, a meni trebaju sva polja iz CompanyProfile.
        // Da mi treba samo jedan element, moram opet <CompanyProfile[]>, pa onda response.data[i] za zelejeni element.
        const response = await axios.get<CompanyProfile[]>(`https://financialmodelingprep.com/api/v3/profile/${query}?apikey=${process.env.REACT_APP_API_KEY}`) 
        // U Axios GET Request, samo Header moze, a Body ne moze. Nemam header, jer mi header tj JWT ne treba, jer ovo je public API sa neta.

        return response; // Full response AxiosResponse<CompanyProfile[]> tipa ili undefinded ako backend Endpoint vrati error jer onda idem u catch block. 
    
    } catch (err) {
        /* Obzirom da ovde gadjam FinancialModelingPrep API koji je public na netu tj nisam ga ja pravio, u catch bloku ne koristim handleError jer je ta funkcija pravljena za moj licni backend. 
        vec koristim obican isAxiosError koji moze biti Network/HTTP/Unexpected . */

        if (axios.isAxiosError(err)){ // HTTP i Network error imaju err.message 
            console.log("Err message:", err.message)
        } 
        else{                        // Unexpected error koji nema err.message
            console.log("Unexpected error", err);
        }
        // Nema return u catch i zato u CompanyPage.tsx getProfileClient mora result?.data[i] (ili veliki if kao u useAuthContext.tsx loginUser), jer ako bude error response=undefined u try block.
    }
}

// For CompanyProfile.tsx
export const getKeyMetrics = async (query: string) => {
    // try-catch zbog axios. Try se izvrsi ako StatusCode=2XX from backend. Catch se izvrsi ako StatusCode!=2XX from backend.
    try{
        // Nije potrebno JWT slati, jer ovo je API sa neta i public je. 
        // Moram navesti <CompanyKeyMetrics[]> kao POTENCIJALNI return ako backend ne vrati error, jer uspesan ovaj Endpoint sa FinancialModelingPrep vraca niz CompanyKeyMetrics tipa, a meni trebaju sva polja iz CompanyKeyMetrics.
        // Da mi treba samo jedan element, moram opet <CompanyKeyMetrics[]>, pa onda response.data[i] za zelejeni element.
        const response = await axios.get<CompanyKeyMetrics[]>(`https://financialmodelingprep.com/api/v3/key-metrics-ttm/${query}?apikey=${process.env.REACT_APP_API_KEY}`)
        // U Axios GET Request, samo Header moze, a Body ne moze. Nemam header, jer mi header tj JWT ne treba, jer ovo je public API sa neta.

        return response; // Full response AxiosResponse<CompanyKeyMetrics[]> tipa ili undefinded ako backend Endpoint vrati error jer onda idem u catch block.
                        // Ostavio sam namerno bez response.data u return, da bih u CompanyProfile.tsx video kako se obradjuje ovakav slucaj kad ovde nije return response.data
    } catch (err) {
        /* Obzirom da ovde gadjam FinancialModelingPrep API koji je public na netu tj nisam ga ja pravio, u catch bloku ne koristim handleError jer je ta funkcija pravljena za moj licni backend. 
        vec koristim obican isAxiosError koji moze biti Network/HTTP/Unexpected . */

        if (axios.isAxiosError(err)){ // HTTP i Network error imaju err.message 
            console.log("Err message:", err.message)
        } 
        else{                        // Unexpected error koji nema err.message
            console.log("Unexpected error", err);
        }
        //Nema return u catch i zato u CompanyProfile.tsx getCompanyKeyMetrics mora result?.data[i] (ili velik if kao u useAuthContext.tsx loginUser), jer ako bude error, response=undefined u try block.
    }
}

// For IncomeStatemen.tsx
export const getIncomeStatement = async (query: string) => {
    // try-catch zbog axios. Try se izvrsi ako StatusCode=2XX from backend. Catch se izvrsi ako StatusCode!=2XX from backend.
    try{
        // Nije potrebno JWT slati, jer ovo je API sa neta i public je. 
        // Moram navesti <CompanyIncomeStatement[]> kao POTENCIJALNI return ako backend ne vrati error, jer uspesan ovaj Endpoint sa FinancialModelingPrep vraca niz CompanyIncomeStatement tipa, a meni trebaju sva polja iz CompanyIncomeStatement.
        // Da mi treba samo jedan element, moram opet <CompanyIncomeStatement[]>, pa onda response.data[i] za zelejeni element.
        const response = await axios.get<CompanyIncomeStatement[]>(`https://financialmodelingprep.com/api/v3/income-statement/${query}?limit=40&apikey=${process.env.REACT_APP_API_KEY}`)
        // U Axios GET Request, samo Header moze, a Body ne moze. Nemam header, jer mi header tj JWT ne treba, jer ovo je public API sa neta.

        return response; // Full response AxiosResponse<CompanyIncomeStatement[]> tipa ili undefinded ako backend Endpoint vrati error jer onda idem u catch block. 
                         // Ostavio sam namerno bez response.data u return, da bih u IncomeStatement.tsx video kako se obradjuje ovakav slucaj kad ovde nije return response.data
    } catch (err) {
        /* Obzirom da ovde gadjam FinancialModelingPrep API koji je public na netu tj nisam ga ja pravio, u catch bloku ne koristim handleError jer je ta funkcija pravljena za moj licni backend. 
        vec koristim obican isAxiosError koji moze biti Network/HTTP/Unexpected . */

        if (axios.isAxiosError(err)){ // HTTP/Network error imaju err.message 
            console.log("Err message:", err.message)
        } 
        else{                        // Unexpected error koji nema err.message
            console.log("Unexpected error", err);
        }
        //Nema return u catch i zato u IncomeStatement.tsx fetchIncomeStatement mora result?.data (ili veliki if kao u useAuthContext.tsx loginUser), jer ako bude error, response=undefined u try block.
    }
}

// For BalanceSheet.tsx
export const getBalanceSheet = async (query: string) => {
    // try-catch zbog axios. Try se izvrsi ako StatusCode=2XX from backend. Catch se izvrsi ako StatusCode!=2XX from backend.
    try{
        // Nije potrebno JWT slati, jer ovo je API sa neta i public je. 
        // Moram navesti <CompanyBalanceSheet[]> kao POTENCIJALNI return ako backend ne vrati error, jer uspesan ovaj Endpoint sa FinancialModelingPrep vraca niz CompanyBalanceSheet tipa, a meni trebaju sva polja iz CompanyBalanceSheet.
        // Da mi treba samo jedan element, moram opet <CompanyBalanceSheet[]>, pa onda response.data[i] za zelejeni element.
        const response = await axios.get<CompanyBalanceSheet[]>(`https://financialmodelingprep.com/api/v3/balance-sheet-statement/${query}?limit=40&apikey=${process.env.REACT_APP_API_KEY}`)
        // U Axios GET Request, samo Header moze, a Body ne moze. Nemam header, jer mi header tj JWT ne treba, jer ovo je public API sa neta.

        return response; // Full response AxiosResponse<CompanyBalanceSheet[]> tipa ili undefinded ako backend Endpoint vrati error jer onda idem u catch block. 
                         // Ostavio sam namerno bez response.data ovde, da bih BalanceSheet.tsx video kako se obradjuje ovakav slucaj kad ovde nije return response.data
    } catch (err) {
        /* Obzirom da ovde gadjam FinancialModelingPrep API koji je public na netu tj nisam ga ja pravio, u catch bloku ne koristim handleError jer je ta funkcija pravljena za moj licni backend. 
        vec koristim obican isAxiosError koji moze biti Network/HTTP/Unexpected . */

        if (axios.isAxiosError(err)){ // HTTP/Network error imaju err.message 
            console.log("Err message:", err.message)
        } 
        else{                        // Unexpected error koji nema err.message
            console.log("Unexpected error", err);
        }
        //Nema return u catch i zato u BalanceSheet.tsx fethBalanceSheet mora result?.data (ili veliki if kao u useAuthContext.tsx loginUser), jer ako bude error, response=undefined u try-block.

    }
}

// For Cashflow.tsx
export const getCashflow = async (query: string) => {
    // try-catch zbog axios. Try se izvrsi ako StatusCode=2XX from backend. Catch se izvrsi ako StatusCode!=2XX from backend.
    try{
        // Nije potrebno JWT slati, jer ovo je API sa neta i public je. 
        // Moram navesti <CompanyCashFlow[]> kao POTENCIJALNI return ako backend ne vrati error, jer uspesan ovaj Endpoint sa FinancialModelingPrep vraca niz CompanyCashFlow tipa, a meni trebaju sva polja iz CompanyCashFlow.
        // Da mi treba samo jedan element, moram opet <CompanyCashFlow[]>, pa onda response.data[i] za zelejeni element.
        const response = await axios.get<CompanyCashFlow[]>(`https://financialmodelingprep.com/api/v3/cash-flow-statement/${query}?limit=40&apikey=${process.env.REACT_APP_API_KEY}`)
        // U Axios GET Request, samo Header moze, a Body ne moze. Nemam header, jer mi header tj JWT ne treba, jer ovo je public API sa neta.

        return response; // Full response AxiosResponse<CompanyCashFlow[]> tipa ili undefinded ako backend Endpoint vrati error jer onda idem u catch block. 
                        // Ostavio sam namerno bez response.data ovde, da bih Cashflow.tsx video kako se obradjuje ovakav slucaj kad nije return response.data
    } catch (err) {
        /* Obzirom da ovde gadjam FinancialModelingPrep API koji je public na netu tj nisam ga ja pravio, u catch bloku ne koristim handleError jer je ta funkcija pravljena za moj licni backend. 
        vec koristim obican isAxiosError koji moze biti Network/HTTP/Unexpected . */

        if (axios.isAxiosError(err)){ // HTTP i Network error imaju err.message 
            console.log("Err message:", err.message)
        } 
        else{                        // Unexpected error koji nema err.message
            console.log("Unexpected error", err);
        }
        //Nema return u catch i zato u Cashflow.tsx fetCashFlow mora result?.data (ili veliki if kao u useAuthContext.tsx loginUser), jer ako bude error, response=undefined u try-block.

    }
}

// For u TenK.tsx - godisnji 10-K izvestaj koji US Sec trazi
export const getTenK = async (query: string) => {
    // try-catch zbog axios. Try se izvrsi ako StatusCode=2XX from backend. Catch se izvrsi ako StatusCode!=2XX from backend.
    try{
        // Nije potrebno JWT slati, jer ovo je API sa neta i public je. 
        // Moram navesti <CompanyTenK[]> kao POTENCIJALNI return ako backend ne vrati error, jer uspesan ovaj Endpoint sa FinancialModelingPrep vraca niz CompanyTenK tipa, a meni trebaju sva polja iz CompanyTenK.
        // Da mi treba samo jedan element, moram opet <CompanyTenK[]>, pa onda response.data[i] za zelejeni element.
        const response = await axios.get<CompanyTenK[]>(`https://financialmodelingprep.com/api/v3/sec_filings/${query}?type=10-k&page=0&apikey=${process.env.REACT_APP_API_KEY}`)
        // U Axios GET Request, samo Header moze, a Body ne moze. Nemam header, jer mi header tj JWT ne treba, jer ovo je public API sa neta.

        return response; // Full response AxiosResponse<CompanyTenK[]> tipa ili undefined ako backend Endpoint vrati error jer onda idem u catch block. 
                        // Ostavio sam namerno bez response.data ovde, da bih ComparableCompany.tsx video kako se obradjuje ovakav slucaj kad nije ovde return response.data
    } catch (err) {
         /* Obzirom da ovde gadjam FinancialModelingPrep API koji je public na netu tj nisam ga ja pravio, u catch bloku ne koristim handleError jer je ta funkcija pravljena za moj licni backend. 
        vec koristim obican isAxiosError koji moze biti Network/HTTP/Unexpected . */

        if (axios.isAxiosError(err)){ // HTTP i Network error imaju err.message 
            console.log("Err message:", err.message)
        } 
        else{                        // Unexpected error koji nema err.message
            console.log("Unexpected error", err);
        }
        //Nema return u catch i zato u TenK.tsx fetchTenK mora result?.data (ili veliki if kao u useAuthContext.tsx loginUser), jer ako bude error, response=undefined u try-block.
    }
}

