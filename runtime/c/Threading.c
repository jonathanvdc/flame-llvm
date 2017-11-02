#include <pthread.h>

// Gets the size of a 'pthread_mutex_t' structure.
unsigned int pthread_get_mutex_size()
{
    return sizeof(pthread_mutex_t);
}

// Gets the size of a 'pthread_t' structure.
unsigned int pthread_get_thread_size()
{
    return sizeof(pthread_t);
}
