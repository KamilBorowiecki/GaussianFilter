.code
MyProc2 proc
mov rax, [rcx]  
add rax, rdx    
mov [rcx], rax 
ret             
MyProc2 endp
end