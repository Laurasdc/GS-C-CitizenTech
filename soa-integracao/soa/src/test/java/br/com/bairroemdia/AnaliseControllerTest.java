package br.com.bairroemdia;

import br.com.bairroemdia.domain.Analise;
import br.com.bairroemdia.dto.Contratos.*;
import br.com.bairroemdia.exception.FonteIndisponivelException;
import br.com.bairroemdia.service.AnaliseService;
import java.util.List;
import org.junit.jupiter.api.Test;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.boot.test.autoconfigure.web.servlet.WebMvcTest;
import org.springframework.http.MediaType;
import org.springframework.test.context.bean.override.mockito.MockitoBean;
import org.springframework.test.web.servlet.MockMvc;
import static org.springframework.test.web.servlet.request.MockMvcRequestBuilders.*;
import static org.springframework.test.web.servlet.result.MockMvcResultMatchers.*;
import static org.mockito.ArgumentMatchers.any;
import static org.mockito.Mockito.*;

@WebMvcTest
class AnaliseControllerTest {
    @Autowired MockMvc mvc;
    @MockitoBean AnaliseService service;
    @Test void criaCom201ELocation() throws Exception {
        when(service.gerar(any())).thenReturn(new Analise(new FiltroRequest(null,null,null),List.of()).resposta());
        mvc.perform(post("/api/analises").contentType(MediaType.APPLICATION_JSON).content("{}"))
            .andExpect(status().isCreated()).andExpect(header().exists("Location"))
            .andExpect(jsonPath("$.origem").value("CSharp")).andExpect(jsonPath("$.total").value(0));
    }
    @Test void ufInvalidaNaoConsultaFonte() throws Exception {
        mvc.perform(post("/api/analises").contentType(MediaType.APPLICATION_JSON).content("{\"uf\":\"XX\"}"))
            .andExpect(status().isBadRequest());
        verifyNoInteractions(service);
    }
    @Test void fonteOfflineRetorna502() throws Exception {
        when(service.gerar(any())).thenThrow(new FonteIndisponivelException());
        mvc.perform(post("/api/analises").contentType(MediaType.APPLICATION_JSON).content("{}"))
            .andExpect(status().isBadGateway()).andExpect(jsonPath("$.detail").exists());
    }
}
