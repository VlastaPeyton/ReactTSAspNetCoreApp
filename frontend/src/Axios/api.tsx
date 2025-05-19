import axios from "axios";
import { CompanyBalanceSheet, CompanyCashFlow, CompanyIncomeStatement, CompanyKeyMetrics, CompanyProfile, CompanySearch, CompanyTenK } from "../company"; // Jer nema default export u company.d.ts, pa mora {}

interface SearchResponse {
    data: CompanySearch[]; // Niz elemanata of CompanySearch type jer searchCompanies API vraca niz
}

// Obzirom da exportujem >= 2 stvari, moram ovako exportovati na mestu definisanja. 

// Axios korisitm umesto Fetch, jer je bolji i more user-friendly 

// For SearchPage.tsx
export const searchCompanies = async (query: string) => {
    // Znam iz ReactApp da kod axios mora try-catch uvek
    try{
        const response = await axios.get<SearchResponse>(`https://financialmodelingprep.com/api/v3/search-ticker?query=${query}&limit=10&exchange=NASDAQ&apikey=jFxvEbSmaafEAnX1FZnhRe0TUSVpRzzz`); 
        // response.data je niz 
        return response; // Full response, ali obicno zelim payload i onda response.data bi bilo. 
        // Ostavio sam namerno bez response.data u return, da bih u SearchPage.tsx onSearchSubmit video kako se obradjuje ovakav slucaj kad nije return response.data
        // Axios automatski baci HTTP, Network ili Unexpected error ako dodje do greske.
    } catch (err){
        // Axios ima Network and HTPP error i unexpected error.
        if (axios.isAxiosError(err)){ // HTTP/Network error imaju err.message 
            console.log("Err message:", err.message)
            return err.message; // String je ovo 
        } 
        else{                        // Unexpected error koji nema err.message
            console.log("Unexpected error", err);
            return "Unexpected error"; // String je ovo 
        }
        // Zbog return string u catch, u SearchPage.tsx onSearchSubmit moram da proverim typeof(return) == "string", a zbog try provera  Array.isArray(return.data)
    }
};

// For CompanyPage.tsx
export const getCompanyProfile = async (query: string) => {
    // Znam iz ReactApp da kod axios mora try-catch uvek
    try{
        // Msm da nije bolja ideja <CompanyProfile[]>, nego kao u SearchResponse da radim, ali nek bude ovako ovde
        const response = await axios.get<CompanyProfile[]>(`https://financialmodelingprep.com/api/v3/profile/${query}?apikey=jFxvEbSmaafEAnX1FZnhRe0TUSVpRzzz`) 
        // response.data je niz i mora biti <CompanyProfile[]> jer API vraca niz. Sve i da mi treba samo jedan element, mora <CompanyProfile[]>,pa posle cu response.data[i]
        return response; // Full response, ali obicno zelim payload i onda response.data bi bilo. 
        // Ostavio sam namerno bez response.data u return, da bih CompanyPage.tsx video kako se obradjuje ovakav slucaj kad nije return response.data
        // Axios automatski baci HTTP, Network ili Unexpected error ako dodje do greske
    } catch (err) {
        // Axios ima Network and HTPP error i unexpected error.
        if (axios.isAxiosError(err)){ // HTTP/Network error imaju err.message 
            console.log("Err message:", err.message)
        } 
        else{                        // Unexpected error koji nema err.message
            console.log("Unexpected error", err);
        }
        // Nema return u catch i zato u CompanyPage.tsx getProfileClient mora result?.data[0] jer moze i undefined da vrati u try blocku
    }
}

// For CompanyProfile.tsx
export const getKeyMetrics = async (query: string) => {
    // Znam iz ReactApp da kod axios mora try-catch uvek
    try{
        const response = await axios.get<CompanyKeyMetrics[]>(`https://financialmodelingprep.com/api/v3/key-metrics-ttm/${query}?apikey=jFxvEbSmaafEAnX1FZnhRe0TUSVpRzzz`)
        // response.data je niz i mora biti <CompanyKeyMetrics[]> jer API vraca niz. Sve i da mi treba samo jedan element, mora <CompanyKeyMetrics[]>,pa posle cu response.data[i]
        return response; // Full response, ali obicno zelim payload i onda response.data bi bilo. 
        // Ostavio sam namerno bez response.data u return, da bih CompanyPage.tsx video kako se obradjuje ovakav slucaj kad nije return response.data
        // Axios automatski baci HTTP, Network ili Unexpected error ako dodje do greske
    } catch (err) {
        // Axios ima Network and HTPP error i unexpected error.
        if (axios.isAxiosError(err)){ // HTTP/Network error imaju err.message 
            console.log("Err message:", err.message)
        } 
        else{                        // Unexpected error koji nema err.message
            console.log("Unexpected error", err);
        }
        //Nema return u catch i zato u CompanyProfile.tsx getCompanyKeyMetrics mora result?.data[0] jer moze i undefined da vrati u try block
    }
}

// For IncomeStatemen.tsx
export const getIncomeStatement = async (query: string) => {
    // Znam iz ReactApp da kod axios mora try-catch uvek
    try{
        const response = await axios.get<CompanyIncomeStatement[]>(`https://financialmodelingprep.com/api/v3/income-statement/${query}?limit=40&apikey=jFxvEbSmaafEAnX1FZnhRe0TUSVpRzzz`)
        // response.data je niz i mora biti <CompanyIncomeStatement[]> jer API vraca niz. Sve i da mi treba samo jedan element, mora <CompanyIncomeStatement[]>,pa posle cu response.data[i]
        return response; // Full response, ali obicno zelim payload i onda response.data bi bilo. 
        // Ostavio sam namerno bez response.data u return, da bih CompanyPage.tsx video kako se obradjuje ovakav slucaj kad nije return response.data
        // Axios automatski baci HTTP, Network ili Unexpected error ako dodje do greske
    } catch (err) {
        // Axios ima Network and HTPP error i unexpected error.
        if (axios.isAxiosError(err)){ // HTTP/Network error imaju err.message 
            console.log("Err message:", err.message)
        } 
        else{                        // Unexpected error koji nema err.message
            console.log("Unexpected error", err);
        }
        //Nema return u catch i zato u IncomeStatement.tsx fetchIncomeStatement mora result?.data jer moze i undefined da vrati u try block
    }
}

// For BalanceSheet.tsx
export const getBalanceSheet = async (query: string) => {
    // Znam iz ReactApp da kod axios mora try-catch uvek
    try{
        const response = await axios.get<CompanyBalanceSheet[]>(`https://financialmodelingprep.com/api/v3/balance-sheet-statement/${query}?limit=40&apikey=jFxvEbSmaafEAnX1FZnhRe0TUSVpRzzz`)
        // response.data je niz i mora biti <CompanyBalanceSheet[]> jer API vraca niz. Sve i da mi treba samo jedan element, mora <CompanyBalanceSheet[]>,pa posle cu response.data[i]
        return response; // Full response, ali obicno zelim payload i onda response.data bi bilo. 
        // Ostavio sam namerno bez response.data u return, da bih BalanceSheet.tsx video kako se obradjuje ovakav slucaj kad nije return response.data
        // Axios automatski baci HTTP, Network ili Unexpected error ako dodje do greske
    } catch (err) {
        // Axios ima Network and HTPP error i unexpected error.
        if (axios.isAxiosError(err)){ // HTTP/Network error imaju err.message 
            console.log("Err message:", err.message)
        } 
        else{                        // Unexpected error koji nema err.message
            console.log("Unexpected error", err);
        }
        //Nema return u catch i zato u BalanceSheet.tsx fethBalanceSheet mora result?.data jer moze i undefined da vrati u try block

    }
}

// For Cashflow.tsx
export const getCashflow = async (query: string) => {
    // Znam iz ReactApp da kod axios mora try-catch uvek
    try{
        const response = await axios.get<CompanyCashFlow[]>(`https://financialmodelingprep.com/api/v3/cash-flow-statement/${query}?limit=40&apikey=jFxvEbSmaafEAnX1FZnhRe0TUSVpRzzz`)
        // response.data je niz i mora biti <CompanyCashFlow[]> jer API vraca niz. Sve i da mi treba samo jedan element, mora <CompanyCashFlow[]>,pa posle cu response.data[i]
        return response; // Full response, ali obicno zelim payload i onda response.data bi bilo. 
        // Ostavio sam namerno bez response.data u return, da bih Cashflow.tsx video kako se obradjuje ovakav slucaj kad nije return response.data
        // Axios automatski baci HTTP, Network ili Unexpected error ako dodje do greske
    } catch (err) {
        // Axios ima Network and HTPP error i unexpected error.
        if (axios.isAxiosError(err)){ // HTTP/Network error imaju err.message 
            console.log("Err message:", err.message)
        } 
        else{                        // Unexpected error koji nema err.message
            console.log("Unexpected error", err);
        }
        //Nema return u catch i zato u Cashflow.tsx fetCashFlow mora result?.data jer moze i undefined da vrati u try block

    }
}

//Koristi se u TenK.tsx - godisnji 10-K izvestaj koji US Sec trazi
export const getTenK = async (query: string) => {
    // Znam iz ReactApp da kod axios mora try-catch uvek
    try{
        const response = await axios.get<CompanyTenK[]>(`https://financialmodelingprep.com/api/v3/sec_filings/${query}?type=10-k&page=0&apikey=jFxvEbSmaafEAnX1FZnhRe0TUSVpRzzz`)
        // response.data je niz i mora biti <CompanyTenK[]> jer API vraca niz. Sve i da mi treba samo jedan element, mora <CompanyTenK[]>,pa posle cu response.data[i]
        return response; // Full response, ali obicno zelim payload i onda response.data bi bilo. 
        // Ostavio sam namerno bez response.data u return, da bih ComparableCompany.tsx video kako se obradjuje ovakav slucaj kad nije return response.data
        // Axios automatski baci HTTP, Network ili Unexpected error ako dodje do greske
    } catch (err) {
        // Axios ima Network and HTPP error i unexpected error.
        if (axios.isAxiosError(err)){ // HTTP/Network error imaju err.message 
            console.log("Err message:", err.message)
        } 
        else{                        // Unexpected error koji nema err.message
            console.log("Unexpected error", err);
        }
        //Nema return u catch i zato u TenK.tsx fetchTenK mora result?.data jer moze i undefined da vrati u try block

    }
}

