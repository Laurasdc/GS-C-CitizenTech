package br.com.bairroemdia.exception;

public class FonteIndisponivelException extends RuntimeException {
    public FonteIndisponivelException() {
        super("Não foi possível consultar a API C#. Nenhuma análise foi gravada.");
    }
}
