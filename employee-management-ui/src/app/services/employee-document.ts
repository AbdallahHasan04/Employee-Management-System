import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams, HttpResponse } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface EmployeeDocument
{
    id: number;
    employeeId: number;
    employeeName: string | null;
    documentName: string;
    documentPath: string | null;
    issueDate: string;
    expiryDate: string | null;
    notes: string | null;
    createdBy: string | null;
    creationDate: string;
    modifiedBy: string | null;
    modificationDate: string | null;
}

export interface NewEmployeeDocument
{
    employeeId: number | null;
    documentName: string;
    issueDate: string;
    expiryDate: string | null;
    notes: string | null;
    attachment: File | null;
}

export interface UploadEmployeeDocumentResponse
{
    message: string;
    document: EmployeeDocument;
}

export interface PagedResult<T>
{
    items: T[];
    totalCount: number;
    pageNumber: number;
    pageSize: number;
}

export interface EmployeeDocumentQueryParams
{
    pageNumber: number;
    pageSize: number;
    sortBy?: string;
    sortDescending?: boolean;
    search?: string;
}

@Injectable({ providedIn: 'root' })
export class EmployeeDocumentService {
  private http = inject(HttpClient);
  private apiUrl = 'https://localhost:7038/api/employeedocuments';

    getDocuments(params: EmployeeDocumentQueryParams): Observable<PagedResult<EmployeeDocument>>
    {
        let httpParams = new HttpParams()
            .set('pageNumber', params.pageNumber)
            .set('pageSize', params.pageSize);

        if (params.sortBy) {
            httpParams = httpParams
                .set('sortBy', params.sortBy)
                .set('sortDescending', params.sortDescending ?? false);
        }
        if (params.search) {
            httpParams = httpParams.set('search', params.search);
        }

        return this.http.get<PagedResult<EmployeeDocument>>(this.apiUrl, { params: httpParams });
    }

    uploadDocument(doc: NewEmployeeDocument): Observable<UploadEmployeeDocumentResponse>
    {
        const formData = new FormData();
        formData.append('employeeId', String(doc.employeeId));
        formData.append('documentName', doc.documentName);
        formData.append('issueDate', doc.issueDate);
        if (doc.expiryDate) {
            formData.append('expiryDate', doc.expiryDate);
        }
        if (doc.notes) {
            formData.append('notes', doc.notes);
        }
        if (doc.attachment) {
            formData.append('attachment', doc.attachment);
        }

        return this.http.post<UploadEmployeeDocumentResponse>(this.apiUrl, formData);
    }

    downloadDocument(id: number): Observable<HttpResponse<Blob>>
    {
        return this.http.get(`${this.apiUrl}/${id}/download`, { responseType: 'blob', observe: 'response' });
    }

    deleteDocument(id: number): Observable<{ message: string }>
    {
        return this.http.delete<{ message: string }>(`${this.apiUrl}/${id}`);
    }
}